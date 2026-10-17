# HUD e compartilhamento — 0.8.2

## Implementação
- Código da sala e grupo sempre visíveis durante a partida; vitais e objetivos mais compactos.
- Menu da expedição pelo botão MENU ou Esc: grupo, código, convite copiável, visibilidade real da sala (host), mochila, configurações e saída com confirmação.
- Amigos: consulta à agenda local existente e convite copiável; não há contas, presença ou envio automático de mensagens.
- Volume geral e opacidade dos controles persistidos neste aparelho. Interface respeita o SafeRoot existente.
- Menu bloqueia somente os comandos locais, sem alterar Time.timeScale. A simulação online continua: abrir em lugar seguro.
- Mochila e menu fecham um ao abrir o outro, evitando bloqueio de entrada esquecido.
- Confirmação distingue saída de cliente e encerramento pelo host. Sem migração de host.

## Arquivos
- CampaignSessionMenu.cs (+meta): novo componente construído pelo HUD existente.
- CampaignHud.cs: disposição, código e acesso ao menu.
- CoopInventoryHud.cs: API explícita de abertura/fechamento e coordenação entre modais.
- CampaignHome.cs / Editor/CampaignInstaller.cs: versão 0.8.2, Android código 20.
- Tests/CampaignTests.cs: integração de menu, mochila, configurações e simulação.

## Como jogar com outra pessoa
1. Ambos usam a campanha 0.8.2. No Android, instalar o APK; no PC, abrir pelo atalho atualizado.
2. Host: Campanha → selecionar fase → Criar sala privada.
3. Consultar o código na sala ou, durante a fase, MENU → Copiar convite.
4. Convidado: Campanha → digitar código → Entrar pelo código.
5. Host inicia a fase. Entradas novas durante a fase só até 20% do percurso, limite de quatro; prefira entrar antes de iniciar.
6. Ambos precisam de internet. O host precisa manter o jogo e a sala abertos. Códigos expiram ao encerrar a sala.
7. Pelo WhatsApp, anexar o arquivo APK como Documento. Não enviar somente um atalho do Windows.

## Verificação
Testes PlayMode da rede: 35 casos, 34 passaram, 1 ignorado, zero falhas. O ignorado depende de projeto UGS não vinculado; este projeto está vinculado.
Windows e Android 0.8.2/código 20 produzidos. Windows BuildPipeline: Succeeded, zero erros. Android: exportação Unity seguida de Gradle externo `clean assembleDebug` (problema local Java loopback no Gradle iniciado pela Unity); BUILD SUCCESSFUL, assinatura v2 validada. Instalado via `adb install -r`, preservando dados.

Inspeção real: PC 1280×720 e moto g15 2400×1080. Menu, grupo, código e convite copiável conferidos. Dois clientes Windows por Relay permaneceram sincronizados com menu aberto. Depois PC host + Android cliente entraram por código e carregaram a mesma fase; Android mostrou grupo 2/4 e cópia do código, com simulação no PC continuando. Após retomar o jogo, o joystick moveu o cliente Android e as posições foram refletidas no host; sessão permaneceu conectada durante vários minutos. Não houve teste em redes distintas nem partida completa com o irmão.

Primeira tentativa com Android como host caiu: timeout de serviços e `send queue full`, desconexão de participante. Retentativa com PC host funcionou. Não atribuí causa definitiva nem alterei transporte sem diagnóstico. Recomenda-se PC como host neste teste; estabilidade de host Android e recuperação da interface após queda permanecem pendentes. No encerramento de um teste também houve aviso do anúncio `session not found`, com sala local encerrada. Atalho Esc implementado via Input System; teste físico por automação de teclado ficou inconclusivo por perda de captura/foco, sem afirmar aprovação desse teste.

Arquivo de envio: `C:/Users/Fabricio_S/Desktop/Jogos/Enviar para meu irmao/ForjaDasMares-0.8.2-Android.apk` (51.866.278 bytes).
SHA-256: `4049B373C17D7B38C535B1804EB48D92E278FCA82D7BF08AE5754A686CE9A3CA`.
É build de desenvolvimento ARM64/min API25, não publicação Play Store. O atalho da campanha aponta para CampaignWindows-0.8.2 sem argumentos de teste.
