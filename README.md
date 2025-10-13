# ACC-CLI (Auto Commit CLI)

**ACC-CLI** é uma ferramenta de linha de comando em C#/.NET 8 que automatiza o ciclo de versionamento Git seguindo o padrão Semantic Versioning (SemVer):

- `git add .`
- `git commit`
- `git tag`
- `git push`

Todas as configurações e credenciais ficam em um único arquivo JSON. Ideal para uso em ambientes Linux.

---

## 🛠️ Pré-requisitos

- **Sistema**: Debian 12 (ou equivalente Linux x64 com glibc ≥ 2.31)
- **.NET SDK**: .NET 8.0 SDK
- **Git**: Git ≥ 2.20 instalado e configurado

Instale o .NET 8 no Debian 12:
```bash
wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

---

## 🚀 Instalação

### Empacotar e instalar como DotNet Tool global

1. No diretório raiz do projeto:
   ```bash
   cd ~/Documentos/ACC-CLI/src
   dotnet restore
   dotnet build ACC-CLI.csproj -c Release
   dotnet pack  ACC-CLI.csproj -c Release
   ```
   Isso gerará `bin/Release/autocli.<versão>.nupkg`.

2. Desinstale versões antigas (se houver):
   ```bash
   dotnet tool uninstall --global autocli || true
   ```

3. Instale a nova versão:
   ```bash
   dotnet tool install --global \
     --add-source ./bin/Release \
     autocli --version <versao atual no <Version> do .csproj>
   ```

4. Garanta que `~/.dotnet/tools` esteja no seu `PATH` (adicionar em `~/.profile` se necessário):
   ```bash
   export PATH="$PATH:$HOME/.dotnet/tools"
   source ~/.profile
   ```

5. Verifique:
   ```bash
   autocli --version
   ```

### (Opcional) Instalação via Manifest Local

1. Dentro de `src/`:
   ```bash
   cd ~/Documentos/ACC-CLI/src
   dotnet new tool-manifest
   dotnet tool install autocli --add-source ./bin/Release --version 1.0.3
   ```
   fique atento a versão do .csprod, ela deve ser a mesma aqui neste comando.

2. Execute via manifest:
   ```bash
   dotnet tool run autocli -- --help
   ```

---

## ⚙️ Configuração

Antes de usar, configure suas credenciais Git (HTTP/PAT) apenas uma vez:
```bash
autocli init --username <seu-usuario> --password <seu-pat>
```
Isso criará `~/.autocli/config.json` com permissão restrita e, se o Git não possuir `user.name`/`user.email`, preencherá automaticamente usando os dados informados (ou fallbacks seguros).

---

## 📋 Comandos

### `autocli init`
Cria ou atualiza o arquivo de configuração.

```bash
autocli init --username foo \
             --password ghp_XXXXXXXXXXXXXXXX \
             --git-name "Foo Bar" \
             --git-email foo@example.com
```

Se `--git-name` ou `--git-email` não forem informados, o CLI salvará apenas usuário/senha e, quando necessário, preencherá o Git com o próprio `--username` e um e-mail `@users.noreply.github.com` gerado automaticamente. As informações fornecidas ficam registradas no `config.json` e serão reutilizadas para garantir que `autocli commit` rode com a mesma identidade.

### `autocli commit`
Executa o fluxo completo: add → commit → tag → push.

**Opções**:
- `--minor` → incrementa versão minor (ex: 1.2.3 → 1.3.0)
- `--major` → incrementa versão major (ex: 1.2.3 → 2.0.0)
- `-m "msg"` → mensagem customizada de commit
- `--dry-run` → simula sem alterar nada

```bash
# Exemplo:
autocli commit -m "Correção de bug"      # patch padrão
autocli commit --minor                  # incrementa minor
autocli commit --major -m "Breaking"   # incrementa major
autocli commit --dry-run                # simula apenas
```
---

## 📖 Como funciona

1. **Detecta** se o diretório atual é um repositório Git, caso contrário aborta com código 10.
2. **Staging** automático de todos os arquivos.
3. **Cálculo** da próxima versão SemVer (patch/minor/major).
4. **Commit** com assinatura do usuário Git; se a identidade estiver ausente, o CLI reaplica o nome/e-mail definidos via `autocli init`.
5. **Tag** anotada com `vX.Y.Z`, assinada com o mesmo usuário.
6. **Push** do branch atual e da tag específica.

---
Se caso aparecer um erro: "Erro desconhecido: not a valid reference 'refs/tags/*'"
fique tranquilo que funcionou kkkk

---

## 📄 Licença

MIT © Yann
