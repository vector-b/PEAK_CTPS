# CTPS (Carteira de Trabalho e Previdência Social)

Mod para PEAK que substitui o passaporte por uma Carteira de Trabalho brasileira (CTPS).

**Replaces the Passport with a Brazilian CTPS Work Permit**

## Funcionalidades

- ✅ Substitui a textura 3D do passaporte pela CTPS
- ✅ Substitui o ícone do HUD pela CTPS

## Instalação

1. Instale o [BepInEx](https://thunderstore.io/c/peak/p/BepInEx/BepInExPack_PEAK/)
2. Extraia o mod na pasta `BepInEx/plugins/`
3. Execute o jogo!

## Desenvolvimento

Este mod foi criado usando o [BepInEx Template](https://github.com/Hamunii/BepInEx-Mod-Template).

### Build

```sh
dotnet build
```

### Thunderstore Package

```sh
dotnet build -c Release -target:PackTS -v d
```

O pacote será gerado em `artifacts/thunderstore/`.
