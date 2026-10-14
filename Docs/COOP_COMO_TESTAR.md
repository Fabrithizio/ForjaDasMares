# Cooperativo privado — Windows e Android

## Abrir

Projeto Unity6000.3.26f1: Assets/ForjaDasMares/Scenes/CoopDungeon.unity. Compilar/abrir e pressionar Play. Executável próprio: Builds/CoopWindows/ForjaDasMaresCoop.exe. APK de desenvolvimento ARM64: Builds/CoopAndroid/ForjaDasMaresCoop.apk, pacote com.forjadasmares.prototype. Preservar o app/dados ao atualizar; não limpar armazenamento para instalar.

O modo local anterior continua em Builds/Windows/ForjaDasMares.exe e no botão JOGAR LOCAL. Cenas ArenaTest/DungeonTest preservadas. O cooperativo usa protocolo2: não misturar com APK0.6.0.

## Mesma rede

1. PC e Android na mesma rede. PC escolhe herói e CRIAR SALA LOCAL; informa aos amigos seu IPv4 local, porta7777 e código da sala.
2. Celular escolhe herói, digita esse IP/código/porta e ENTRAR POR IP E CÓDIGO. O host vê jogadores conectados, até4 contando com ele.
3. Host INICIAR PARTIDA. Cada cliente confirma o mesmo mapa antes de receber comandos de combate.
4. PC: WASD/setas, mouse/ataque, habilidade/esquiva conforme ArenaControls; E/F para interagir. Android: joystick à esquerda, ATAQUE/ESQUIVA/HABILIDADE/INTERAGIR à direita. Mira de toque segue direção do movimento; mira automática completa ainda pertence à interface posterior.
5. INVENTÁRIO consulta o perfil privado da sessão e envia intents de equipagem. Gerenciar fora de encontros/ações. RESULTADO mostra a última conclusão gravada pelo host. Pausar por janela de inventário bloqueia só suas entradas.
6. Herói morto aguarda vitória da sala; reaparece com50% da vida. Toda equipe morta provoca derrota. Sair host encerra sala, sem migração. Retornar ao mesmo host dentro de90s após desconexão reconhecida reserva a vaga/estado. Timeout de transporte pode demorar a reconhecer uma queda abrupta.

Não alterar/expor portas ou firewall automaticamente. Este procedimento foi executado neste PC com moto g15 na LAN; redes com isolamento de clientes podem impedir comunicação. O teste não comprova acesso pela internet.

## Duas ou mais instâncias Windows

Fornecer -netIdentity com GUIDN distinto para cada processo. Exemplo de argumentos do host: -netMode host -netIdentity 00000000000000000000000000000001 -netPin TEST42. Cliente: -netMode client -netIdentity 00000000000000000000000000000002 -netAddress 127.0.0.1 -netPin TEST42. Identidade padrão persiste por dispositivo/usuário e não serve para duas instâncias simultâneas sem override. Não usar os IDs de exemplo para sessão pública/comercial.

## Internet e limites

Vincular um projeto real pela Unity Services usando sua conta/organização; configurar ambiente, Authentication e serviços Sessions/Lobby/Relay. Conferir quotas/termos/custos antes de habilitar cobrança. Ver REDE_PLANO_MIGRACAO.md. Os botões de internet executam o SDK, mas bloqueiam com mensagem quando projectId está vazio. Nenhuma chave secreta é exigida no cliente e nenhuma credencial deve ser enviada no chat. Este projeto ainda não tem vínculo; internet não foi validada.

Mapa cooperativo deste recorte: catacumbas fixas/seed12345. Outros mapas/biomas continuam no modo local. Inventário/HUD são interfaces mínimas, sem afirmar que a etapa completa de UI foi entregue. Perfis cooperativos são armazenados no host, separados do save offline; backup do Git contém desenvolvimento, não esses saves privados. Host/identidade GUID não são segurança comercial de economia. Sem migração de host, matchmaking público ou monetização.

## Builds e diagnóstico

Menu Forja das Mares/Install private coop variant repara composição CoopDungeon quando necessário. Métodos de build: ForjaDasMares.Networking.Editor.CoopInstaller.Windows com destino ativo Win64; Android com destino ativo Android. Não compilar dois Editores simultaneamente no mesmo projeto. Na máquina atual Gradle interno apresenta erro Java NIO de loopback: documentação do teste físico registra montagem direta, restrita ao processo. Nunca entregar APK anterior como se fosse a última compilação.

Logs Windows: Application.persistentDataPath/Player.log; Android: log do processo do próprio app via adb. Console deve permanecer disponível, sem ocultar exceções. Em caso de cenário preto/SSAO, confirmar destino Win64 ativo ao coletar recursos e os registros canônicos URPGlobalSettings. Configuração Android pode remover temporariamente dois registros SSAO do fonte; restaurar após sair do Editor, antes do próximo build Windows.
