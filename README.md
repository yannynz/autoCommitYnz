# ACC-CLI (Auto Commit CLI)

**ACC-CLI** e uma ferramenta de linha de comando em C#/.NET 8 que automatiza o fluxo:

- `git add .`
- `git commit`
- `git tag`
- `git push`

O versionamento segue SemVer (`major.minor.patch`) usando tags no formato `vX.Y.Z`.

## Pre-requisitos

### Comuns

- Git `>= 2.20`
- .NET SDK `8.0`
- Repositorio Git com remote `origin` configurado
- Credencial HTTPS/PAT valida para push

### Linux

- Debian/Ubuntu recomendados (script tenta instalar .NET 8 automaticamente nesses sistemas)
- `sudo` (se precisar instalar dependencias)
- `wget` ou `curl` (para baixar pacote da Microsoft, quando necessario)

### Windows

- PowerShell 5.1+ ou PowerShell 7+
- Git for Windows
- .NET SDK 8.0

## Instalacao (recomendada)

Clone o repositorio:

```bash
git clone https://github.com/yannynz/autoCommitYnz.git
cd autoCommitYnz
```

### Linux

```bash
chmod +x scripts/install-linux.sh
./scripts/install-linux.sh
```

### Windows (PowerShell)

```powershell
git clone https://github.com/yannynz/autoCommitYnz.git
cd autoCommitYnz
Set-ExecutionPolicy -Scope Process Bypass -Force
.\scripts\install-windows.ps1
```

## Atualizacao do app instalado

Sempre que houver nova versao no repositorio, atualize assim:

### Linux

```bash
cd autoCommitYnz
git stash -u # opcional: use se houver mudancas locais nao commitadas
git pull --rebase --tags
./scripts/install-linux.sh
autocli --version
# git stash pop # opcional: reaplica mudancas locais
```

### Windows (PowerShell)

```powershell
cd autoCommitYnz
# git stash -u # opcional: use se houver mudancas locais nao commitadas
git pull --rebase --tags
Set-ExecutionPolicy -Scope Process Bypass -Force
.\scripts\install-windows.ps1
autocli --version
# git stash pop # opcional: reaplica mudancas locais
```

## Instalacao manual

### Linux

```bash
cd src
dotnet restore
dotnet build ACC-CLI.csproj -c Release
dotnet pack ACC-CLI.csproj -c Release

dotnet tool uninstall --global autocli || true
dotnet tool install --global --add-source ./bin/Release autocli --version 1.0.6
```

Garanta `~/.dotnet/tools` no `PATH`.

### Windows (PowerShell)

```powershell
cd .\src
dotnet restore
dotnet build ACC-CLI.csproj -c Release
dotnet pack ACC-CLI.csproj -c Release

dotnet tool uninstall --global autocli
dotnet tool install --global --add-source .\bin\Release autocli --version 1.0.6
```

Garanta `$HOME\.dotnet\tools` no `PATH`.

## Configuracao inicial

Configure credenciais uma vez:

```bash
autocli init --username <seu-usuario> --password <seu-pat>
```

Opcionalmente defina identidade Git explicita:

```bash
autocli init --username <seu-usuario> --password <seu-pat> \
             --git-name "Seu Nome" --git-email "voce@exemplo.com"
```

Arquivo salvo em:

- Linux: `~/.autocli/config.json`
- Windows: `%USERPROFILE%\\.autocli\\config.json`

## Comandos

### `autocli commit`

Fluxo: add -> commit -> tag -> push

Opcoes:

- `--minor`: incrementa `minor` e zera `patch`
- `--major`: incrementa `major` e zera `minor`/`patch`
- `-m "mensagem"`: mensagem customizada
- `--dry-run`: simula sem alterar nada

Exemplos:

```bash
autocli commit -m "Correcao de bug"
autocli commit --minor
autocli commit --major -m "Breaking change"
autocli commit --dry-run
```

## Uso em 2 maquinas (sincronizacao remota)

O `autocli commit` agora faz validacoes para evitar conflito de versao entre maquinas:

1. Executa `fetch` no remote (`origin`) incluindo `refs/tags/*`.
2. Le a maior tag SemVer disponivel apos o fetch para calcular a proxima versao.
3. Bloqueia execucao quando o branch local esta atrasado/divergente do upstream remoto.

Se aparecer erro de branch atrasado/divergente, rode:

```bash
git pull --rebase --tags
```

e execute o `autocli commit` novamente.

## Solucao de problemas

- `autocli: command not found`:
  - Linux: adicione `~/.dotnet/tools` ao `PATH`
  - Windows: adicione `%USERPROFILE%\\.dotnet\\tools` ao `PATH`
- Erro de autenticacao no push:
  - confira usuario/PAT do `autocli init`
- Tag ja existe:
  - sincronize com `git pull --rebase --tags` e rode de novo
- Branch foi enviado mas tag nao:
  - rode `git push origin refs/tags/vX.Y.Z` para enviar apenas a tag pendente

## Licenca

MIT
