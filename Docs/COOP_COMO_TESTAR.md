# Cooperativo privado — Windows e Android

## Abrir

Projeto Unity6000.3.26f1: Assets/ForjaDasMares/Scenes/CoopDungeon.unity. Compilar/abrir e pressionar Play. Internet vinculada com câmera revisada0.6.3: Builds/InternetWindows-0.6.3/ForjaDasMaresCoop.exe e Builds/InternetAndroid-0.6.3/ForjaDasMaresCoop.apk, pacote com.forjadasmares.prototype. Atalho adicional Jogar ForjaDasMares Online na pasta Jogos utiliza save InternetCoopProfiles separado, para coexistir com a partida LAN antiga sem gravar sobre ela. Binários LAN0.6.1 continuam em CoopWindows/CoopAndroid. Preservar dados ao atualizar.

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

Projeto ForjaDasMares vinculado e ambiente production configurado. Host usa CRIAR SALA INTERNET; cliente informa o código retornado no campo abaixo desse botão e usa ENTRAR POR CÓDIGO; depois o host inicia. Não usar o campo TEST42 de LAN para o código Relay. Não é necessário abrir porta no roteador para Relay. Nenhuma chave secreta exigida no cliente. Teste real Windows–moto g15 em Wi-Fi e reconexão por código executados; detalhes e limites em REDE_INTERNET_2026-10-15.md. Redes distintas/operadoras não testadas porque o titular não tem dados móveis. Configuração para clones/outros ambientes, custos e futura publicação em INTERNET_E_PUBLICACAO.md.

Mapa cooperativo deste recorte: catacumbas fixas/seed12345. Outros mapas/biomas continuam no modo local. Inventário/HUD são interfaces mínimas, sem afirmar que a etapa completa de UI foi entregue. Perfis cooperativos são armazenados no host, separados do save offline; backup do Git contém desenvolvimento, não esses saves privados. Host/identidade GUID não são segurança comercial de economia. Sem migração de host, matchmaking público ou monetização.

## Builds e diagnóstico

Menu Forja das Mares/Install private coop variant repara composição CoopDungeon quando necessário. Métodos vinculados0.6.3: ForjaDasMares.Networking.Editor.CoopInstaller.InternetWindows com destino ativo Win64; InternetAndroid com destino ativo Android. Esses métodos verificam vínculo/configuração e produzem builds de desenvolvimento em pastas separadas. Windows/Android antigos continuam disponíveis para QA LAN. Não executar dois Editores no mesmo projeto. Gradle interno apresenta erro Java NIO de loopback nesta máquina; montagem direta processual registrada no relatório. Não usar APK anterior como se fosse a revisão mais recente.

Logs Windows: Application.persistentDataPath/Player.log; Android: log do processo do próprio app via adb. Console deve permanecer disponível, sem ocultar exceções. Em caso de cenário preto/SSAO, confirmar destino Win64 ativo ao coletar recursos e os registros canônicos URPGlobalSettings. Configuração Android pode remover temporariamente dois registros SSAO do fonte; restaurar após sair do Editor, antes do próximo build Windows.
