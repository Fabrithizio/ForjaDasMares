# 0.25.1 — interações da cidade

30/10/2026. Revisão focada de PC, mantendo código compatível com toque.

- O texto de interação e F agora escolhem o mesmo alvo próximo, pelo menor raio real; componentes desativados não entram na seleção.
- Clique em morador móvel atualiza o destino durante a aproximação, sem exigir cliques repetidos; caminho é recalculado com o limite existente e sem buscas globais por frame.
- Comando de chão, cancelamento por UI e ataque parado continuam preservados.

Arquivos: Networking/World/TownHub.cs; Scripts/Input/DesktopRpgControls.cs; Networking/Tests/TownTests.cs; Networking/Tests/ArpgControlsTests.cs; Networking/Editor/CampaignInstaller.cs (todos sob Assets/ForjaDasMares).

25/25 testes PlayMode passaram em Logs/town-interactions-release-tests.xml. Incluem indicação/F consistentes, exclusão de alvo desativado, morador que muda de lugar depois do clique e regressões dos controles. Teste inicialmente usava alteração direta de Transform e foi corrigido para usar o Teleport real do PlayerMotor.

O APK 0.25.0/code49 foi enviado por WhatsApp para a conversa da própria conta, Mensagens para mim, com confirmação de entrega visível. Não se declara instalação ou teste físico desta versão. Esta revisão 0.25.1 não gera outro APK.

Limitações anteriores continuam: cidade local, NPCs geométricos provisórios, costa ainda exige auditoria própria das colisões e mobile requer nova validação física. Não houve alteração de salvamento, inventário ou economia.

Build Windows real concluída: Logs/town0251-windows.log, CAMPAIGN_BUILD StandaloneWindows64 Succeeded errors=0. Não foi executada sessão online ou teste físico móvel nesta revisão.
