#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
SRC_DIR="${REPO_ROOT}/src"
CSProj="${SRC_DIR}/ACC-CLI.csproj"

log() {
  printf '[install-linux] %s\n' "$1"
}

fail() {
  printf '[install-linux] ERROR: %s\n' "$1" >&2
  exit 1
}

command_exists() {
  command -v "$1" >/dev/null 2>&1
}

ensure_dotnet_sdk_8() {
  if command_exists dotnet && dotnet --list-sdks | awk '{print $1}' | grep -q '^8\.'; then
    return 0
  fi

  if [[ ! -f /etc/os-release ]]; then
    fail ".NET SDK 8.0 nao encontrado e nao foi possivel detectar a distribuicao."
  fi

  # shellcheck disable=SC1091
  source /etc/os-release
  local sudo_cmd=""
  if [[ "${EUID}" -ne 0 ]]; then
    if command_exists sudo; then
      sudo_cmd="sudo"
    else
      fail "sudo nao encontrado. Rode como root ou instale o sudo."
    fi
  fi

  local install_supported=0
  case "${ID:-}" in
    debian|ubuntu)
      install_supported=1
      ;;
  esac

  if [[ "${install_supported}" -ne 1 ]]; then
    fail ".NET SDK 8.0 nao encontrado. Instale manualmente em https://dotnet.microsoft.com/download/dotnet/8.0"
  fi

  log "Instalando .NET SDK 8.0 para ${ID:-unknown} ${VERSION_ID:-unknown}..."
  local pkg="/tmp/packages-microsoft-prod.deb"
  if command_exists wget; then
    wget -q "https://packages.microsoft.com/config/${ID}/${VERSION_ID}/packages-microsoft-prod.deb" -O "${pkg}"
  elif command_exists curl; then
    curl -fsSL "https://packages.microsoft.com/config/${ID}/${VERSION_ID}/packages-microsoft-prod.deb" -o "${pkg}"
  else
    fail "Instale wget ou curl para baixar os pacotes da Microsoft."
  fi

  ${sudo_cmd} dpkg -i "${pkg}" >/dev/null
  ${sudo_cmd} apt-get update >/dev/null
  ${sudo_cmd} apt-get install -y dotnet-sdk-8.0 >/dev/null
}

if [[ ! -f "${CSProj}" ]]; then
  fail "Projeto nao encontrado em ${CSProj}"
fi

if ! command_exists git; then
  fail "Git nao encontrado. Instale Git >= 2.20."
fi

ensure_dotnet_sdk_8

if ! dotnet --list-sdks | awk '{print $1}' | grep -q '^8\.'; then
  fail ".NET SDK 8.0 nao esta disponivel apos a instalacao."
fi

VERSION="$(sed -n 's|.*<Version>\(.*\)</Version>.*|\1|p' "${CSProj}" | head -n 1)"
if [[ -z "${VERSION}" ]]; then
  fail "Nao foi possivel identificar a versao no ACC-CLI.csproj."
fi

log "Compilando e empacotando autocli ${VERSION}..."
pushd "${SRC_DIR}" >/dev/null
dotnet restore
dotnet build ACC-CLI.csproj -c Release
dotnet pack ACC-CLI.csproj -c Release

log "Instalando ferramenta global..."
dotnet tool uninstall --global autocli >/dev/null 2>&1 || true
dotnet tool install --global --add-source "${SRC_DIR}/bin/Release" autocli --version "${VERSION}"
popd >/dev/null

DOTNET_TOOLS_PATH="${HOME}/.dotnet/tools"
if [[ ":${PATH}:" != *":${DOTNET_TOOLS_PATH}:"* ]]; then
  log "Adicionando ${DOTNET_TOOLS_PATH} ao PATH do usuario..."
  if [[ -f "${HOME}/.profile" ]]; then
    if ! grep -Fq "${DOTNET_TOOLS_PATH}" "${HOME}/.profile"; then
      printf '\nexport PATH="$PATH:%s"\n' "${DOTNET_TOOLS_PATH}" >> "${HOME}/.profile"
    fi
  else
    printf 'export PATH="$PATH:%s"\n' "${DOTNET_TOOLS_PATH}" > "${HOME}/.profile"
  fi
  export PATH="${PATH}:${DOTNET_TOOLS_PATH}"
fi

log "Instalacao concluida."
autocli --version
