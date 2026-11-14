# Clareza do inventário — 0.31.1

Escopo: correções na interface existente; nenhum mapa, regra de combate ou conteúdo novo. Sem APK.

- Bloqueios no RPG agora explicam morte, ação em andamento, inimigos próximos ou expedição pendente.
- O mesmo motivo determina a permissão e o texto; estado anterior à inicialização do perfil é tratado sem acesso nulo.
- Mensagem de carregamento/recuperação do ProfileService preservada na interface regional.
- Rodapé regional informa perfil do aparelho compartilhado com desafios e salvamento automático, sem instruções de host fora de contexto.
- Status apresenta velocidade de ataque como bônus percentual e explica que já está incluído na cadência real; fórmulas não mudaram.

Arquivos alterados: Networking/World/RegionalInventory.cs, CoopInventoryHud.cs, CoopInventoryHud.Areas.cs, Tests/RegionalInventoryTests.cs, Editor/CampaignInstaller.cs (sob Assets/ForjaDasMares); ProjectSettings/ProjectSettings.asset (versão 0.31.1).

Validação: 40/40 testes PlayMode aprovados, incluindo UI com personagem morto e ausência de bloqueio quando permitido. Regressões de equipar/desequipar, persistência, viagem, catálogo e atalhos mantidas. Build Windows Succeeded/errors=0/exit0. Executável em Builds/Atual/Windows/ForjaDasMaresCampanha.exe. Nenhum novo teste visual nativo, Android físico ou multiplayer real nesta revisão.

Para conferir: abrir cidade → INVENTÁRIO → selecionar equipamento → equipar/desequipar; STATUS → utilidade → bônus de velocidade de ataque. Em perigo ou morto, a interface deve explicar por que não é possível alterar equipamentos. Não altere saves para simular falhas; testes usam perfis isolados.

Pendente: avaliação visual pelo titular, teclado físico, toque em aparelho e sessões online. APK existente permanece 0.30.0, sem as revisões 0.31.x.