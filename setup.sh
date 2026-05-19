#!/bin/bash
# ═══════════════════════════════════════════════
# PayFlow — Script de criação da solution
# Rode na sua máquina com .NET 8 instalado
# ═══════════════════════════════════════════════

# 1. Criar a solution
dotnet new sln -n PayFlow

# 2. Criar os 4 projetos
dotnet new webapi -n PayFlow.API -o src/PayFlow.API
dotnet new classlib -n PayFlow.Domain -o src/PayFlow.Domain
dotnet new classlib -n PayFlow.Infrastructure -o src/PayFlow.Infrastructure
dotnet new worker -n PayFlow.Worker -o src/PayFlow.Worker

# 3. Criar projetos de teste
dotnet new xunit -n PayFlow.Domain.Tests -o tests/PayFlow.Domain.Tests
dotnet new xunit -n PayFlow.API.Tests -o tests/PayFlow.API.Tests

# 4. Adicionar todos à solution
dotnet sln add src/PayFlow.API/PayFlow.API.csproj
dotnet sln add src/PayFlow.Domain/PayFlow.Domain.csproj
dotnet sln add src/PayFlow.Infrastructure/PayFlow.Infrastructure.csproj
dotnet sln add src/PayFlow.Worker/PayFlow.Worker.csproj
dotnet sln add tests/PayFlow.Domain.Tests/PayFlow.Domain.Tests.csproj
dotnet sln add tests/PayFlow.API.Tests/PayFlow.API.Tests.csproj

# 5. Configurar referências entre projetos
# API depende de Domain e Infrastructure
dotnet add src/PayFlow.API/PayFlow.API.csproj reference src/PayFlow.Domain/PayFlow.Domain.csproj
dotnet add src/PayFlow.API/PayFlow.API.csproj reference src/PayFlow.Infrastructure/PayFlow.Infrastructure.csproj

# Infrastructure depende de Domain
dotnet add src/PayFlow.Infrastructure/PayFlow.Infrastructure.csproj reference src/PayFlow.Domain/PayFlow.Domain.csproj

# Worker depende de Domain e Infrastructure
dotnet add src/PayFlow.Worker/PayFlow.Worker.csproj reference src/PayFlow.Domain/PayFlow.Domain.csproj
dotnet add src/PayFlow.Worker/PayFlow.Worker.csproj reference src/PayFlow.Infrastructure/PayFlow.Infrastructure.csproj

# Testes dependem dos projetos que testam
dotnet add tests/PayFlow.Domain.Tests/PayFlow.Domain.Tests.csproj reference src/PayFlow.Domain/PayFlow.Domain.csproj
dotnet add tests/PayFlow.API.Tests/PayFlow.API.Tests.csproj reference src/PayFlow.API/PayFlow.API.csproj

echo ""
echo "✅ Solution criada com sucesso!"
echo "Rode: dotnet build para verificar"
