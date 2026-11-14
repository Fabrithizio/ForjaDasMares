# Joystick flutuante limitado — 06/11/2026

Implementado em TouchMoveStick e FloatingStickArea, reutilizado pelos HUDs existentes.
- Toque inicia o controle na área inferior esquerda (42% da largura e 45% da altura do painel seguro, com margens).
- A base fica inteiramente dentro da zona; não segue o dedo durante o arraste.
- Primeiro toque é neutro mesmo perto da borda. Arraste com diagonal normalizada e zona morta.
- Um dedo é dono do gesto; outros dedos não o soltam. Soltar, desativar, perder foco ou pausar libera a entrada; mudança de resolução durante gesto também libera.
- Visual discreto em repouso, opaco enquanto usado. Campos ajustáveis no Inspector.

Validação na Unity/PC: compilação realizada. Rodada de integração: 46/47 passaram; o teste restante lia o movimento processado no mesmo instante do evento de soltura, antes da atualização do adaptador de desktop. Ajustado para verificar entrada bruta imediatamente e movimento processado no quadro seguinte. Resultado da confirmação consta em Logs/joystick-floating-confirmed.xml.
Um teste legado de ArenaAcceptanceTests exige WASD e falha com o controle por mouse atual; não foi marcado como aprovado nem removido.

Celular indisponível: esta alteração ainda não foi testada fisicamente nem incluída em novo APK. A instalação anterior é 0.27.0. Não foi gerado APK adicional.

Arquivos: Scripts/Input/TouchMoveStick.cs, Scripts/Input/FloatingStickArea.cs, Networking/Tests/FloatingStickTests.cs e Tests/PlayMode/HeroAcceptanceTests.cs.
`nConfirmação final: 4/4 testes passaram, zero falhas; Unity encerrou com código 0. Somados aos testes distintos aprovados na rodada anterior, os 47 casos daquele filtro estão cobertos; não significa auditoria completa do jogo.
