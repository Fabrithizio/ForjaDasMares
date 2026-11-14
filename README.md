# Forja das Marés

RPG de ação para PC e celular (Android), com cidades, regiões abertas e instâncias cooperativas para até 4 jogadores.
Unity 6.3 LTS (6000.3.26f1), URP, C#, Input System, Netcode for GameObjects + Unity Relay/Lobby.

## Cenas do jogo

| Cena | O que é |
|---|---|
| `CampaignCoop` | Menu principal e expedições cooperativas (protótipo da instância). |
| `RefugioUltimoSino` | Primeira cidade. |
| `CostaAto1` | Região aberta com inimigos. |
| `CatacumbasDasMares` | Instância curta das Catacumbas, com cartas de prêmio. |
| `CostaDasRuinas` | Região-piloto usada só pelos testes automáticos. Não entra no build. |

## Como abrir e testar

1. Abra o projeto pelo Unity Hub.
2. Abra `Assets/ForjaDasMares/Scenes/CampaignCoop.unity` (ou aperte F6) e aperte Play.
3. Para gerar o executável do Windows: menu **Forja → Build → Gerar Windows (Builds/Atual)**. Sai em `Builds/Atual/Windows`.

## Documentação

- `Docs/PROGRESSO.md` — histórico das entregas.
- `Docs/BASE_DO_MUNDO_2026_10_31.md` — base da história aprovada.
- `Docs/DIRECAO_MAGICA_0_32_0.md` — direção artística.
- `Docs/PROPOSTA_MUNDO_REGIONAL.md` — proposta de mundo compartilhado e servidores.
- `Docs/ARQUITETURA.md` — plano técnico antigo de longo prazo.
- `Docs/Historico/` — relatórios de versões antigas, guardados só para consulta.

Library, Temp, Logs e Builds ficam fora do Git.
