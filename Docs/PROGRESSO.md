# Etapa 3 — entrega atual (13/10/2026)

Quatro heróis configurados e selecionáveis na ArenaTest existente. Guerreiro: espada em arco/combo e impacto; Patrulheiro: projéteis e armadilha; Arcanista: gelo/fogo/raio e pulso glacial; Cavaleiro Rúnico: armadura, proteção do aliado e investida. ScriptableObjects separados; pool de projéteis/zonas; dano, facção, direção, obstáculos, knockback, recuperação/cooldowns e invulnerabilidade finita. Sem equipamento ou multiplayer.

Joystick Android substituído por base fixa, indicador interno e captura de pointerId. Testes verificaram retenção do vetor, liberação de outro dedo e deslocamento do CharacterController. O moto g15 voltou a ser detectado e o APK foi instalado e testado nele por entrada de toque injetada via ADB. Movimento segurado, ataques e habilidades dos quatro heróis, esquiva e reset conferidos em capturas. Avaliação humana de ergonomia e multitoque simultâneo ainda é necessária.

Verificação final: **23 testes PlayMode, 23 aprovados, zero falhas**; testes-delivery.xml. Mantidos os 11 testes da base e adicionados 12 de heróis/efeitos/controles/apresentação, incluindo reset da cadeia e liberação da direção ao trocar herói. Primeiro corrigida a criação de MaterialPropertyBlock no construtor. Depois corrigidos colisão do aliado com o percurso de teste e raio da explosão de fogo. A rodada final inclui movimento pelo joystick, investida/dano único/knockback e poses de dano/morte/vitória.

Não há clips reais ou modelos finais. Idle/Walk/Attack/Skill/Dodge/Hit/Dead/Victory são estados de placeholders procedurais, com pontos de integração de Animator documentados. Animation integrado 1.0.0 habilitado. VS Code continua o editor; Visual Studio IDE não foi instalado.

Guia: Docs/HEROIS_COMO_TESTAR.md. Catálogo: Docs/ARQUIVOS_ETAPA3.txt. Próxima etapa de inimigos depende de nova instrução. Histórico abaixo descreve checkpoints anteriores.

---
# Progresso — Forja das Marés

## Entrega atual

Etapas 1 e 2: projeto auditado/configurado e primeira arena jogável concluída. Alvos atuais: Windows e Android; iOS adiado.

Unity 6.3 LTS 6000.3.26f1, URP 17.3.0, Input System 1.14.2 e uGUI 2.0.0. A cena real salva é `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, incluída nas configurações de build. A licença do Editor foi validada. VS Code e extensões C#/Unity instalados; Visual Studio IDE não foi instalado.

## Implementado

- Arena com chão, limites, obstáculos, luz, spawn seguro e placeholders geométricos originais.
- Movimento em oito direções, diagonal normalizada, aceleração/desaceleração, colisão com CharacterController e esquiva.
- Entrada separada pelo Input System: teclado/mouse, bindings de gamepad e controles mínimos de toque.
- Câmera diagonal suave, enquadramento por proporção de tela e tentativa de evitar obstáculos por inclinação/colisão.
- Ataque próximo com alcance/arco, preparação/recuperação, bloqueio por parede e dano sem duplicação por collider.
- Alvo com vida, flash de acerto, barra no HUD e reset da arena por R/botão.
- Dado de arma em ScriptableObject, dependências explícitas na cena e componentes separados para apresentação/lógica/entrada.

## Verificação executada

O Editor importou o projeto, compilou os assemblies e gerou a cena com `ARENA_BUILD_OK`. PlayMode: **11 testes executados, 11 aprovados, zero falhas**, registrados em `arena-test-results.xml` na entrega. Foram verificados movimento/diagonal, variação de taxa de quadros, paredes/esquiva, obstáculos, dano/cooldown/reset, bloqueio de golpe, enquadramento, componentes/referências, entrada real do Input System com dispositivos sintéticos, acompanhamento suave e controles virtuais de toque.

A primeira execução apresentou uma falha de entrada de teclado sintético em batch sem foco de Game. O teste passou a configurar temporariamente o encaminhamento de entrada do Editor para testes sem foco e restaurar a configuração depois. A segunda execução completa aprovou os 11 testes. Isso não representa uma sessão física com teclado/gamepad ou telefone Android.

## Plano por etapa

| Etapa | Estado |
| --- | --- |
| 1. Auditoria/arquitetura | Concluída para esta base local; arquitetura futura documentada |
| 2. Arena | Concluída e validada no Editor por 11 testes de PlayMode |
| 3. Heróis/habilidades | Não iniciada |
| 4. Inimigos | Não iniciada |
| 5. Salas/masmorras | Não iniciada |
| 6. Progressão/saques | Não iniciada |
| 7. Multiplayer | Não iniciada; revalidar NGO/Transport/serviços antes de integrar |
| 8. Interface/controles completos | Não iniciada; somente HUD/controles mínimos da arena |
| 9. Conteúdo | Não iniciada |
| 10. Builds/validação em aparelhos | Não iniciada; testes necessários acompanham cada entrega |

## Arquivos criados e alterados

Criados: template oficial Universal 3D em Assets/Packages/ProjectSettings, `.gitignore`, pasta `Assets/ForjaDasMares` com cena, materiais, arma, input actions, assemblies, componentes, ferramentas de geração/captura e testes; `Docs/ARENA_COMO_JOGAR.md` e catálogo `Docs/ARQUIVOS_ETAPA2.txt`.

Alterados: os quatro documentos anteriores (`LEIA-ME.md`, `Docs/AUDITORIA.md`, `Docs/ARQUITETURA.md`, `Docs/PROGRESSO.md`); manifest/lock; configurações de versão, player, layers, render pipeline e build scenes do template. Os arquivos do jogo estão catalogados em `ARQUIVOS_ETAPA2.txt`.

## Limitações e dependências restantes

Não há multiplayer, progressão, inventário, inimigos ou conteúdo comercial nesta entrega. Os placeholders não são modelos/animações finais. Foi gerado APK ARM64 e realizado teste funcional no moto g15. Ainda faltam a matriz de aparelhos, profiling e validação de distribuição; não há crossplay implementado ou comprovado. Perfil de desempenho e ajustes de toque/safe area/acessibilidade completos precisam de testes em dispositivos. Não há áudio nesta arena. A câmera foi validada em obstáculos baixos desta cena; salas fechadas precisam de nova avaliação.

Na etapa 7 serão necessários projeto UGS, autenticação/serviços e validação real de sessões. iOS permanece fora do escopo atual. Backend/economia e autoridade comercial serão definidos nas etapas correspondentes.

## Executar e próxima etapa

Abrir o projeto pelo Unity Hub, abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, clicar Play e dar foco à janela Game. WASD/setas movem, mouse mira, clique esquerdo ataca, Espaço esquiva e R reinicia. Guia detalhado: `ARENA_COMO_JOGAR.md`.

Aguardar a próxima instrução do usuário antes de iniciar heróis/habilidades ou outros sistemas.

## Conferência visual

Captura da cena real em PlayMode gerada e inspecionada: chão/limites, luz/sombras, jogador ciano com espada geométrica, alvo laranja, obstáculos e HUD visíveis. Foi corrigida a renderização de cores desativando GPU Resident Drawer/SRP Batcher nos perfis desta arena. Essas otimizações devem ser reavaliadas em profiling. A limitação de indexação interna do Editor encontrada na primeira captura está documentada em AUDITORIA.md.

## Ambiente instalado e conferido

Unity Hub, Unity 6.3 LTS 6000.3.26f1, VS Code com C#/C# Dev Kit/Unity/.NET Install Tool e Git instalados. A solução do projeto foi gerada para o editor de código. Não foi instalado Visual Studio IDE.

Android Build Support, OpenJDK 17.0.18+8, NDK r27c (27.2.12479018), CMake 3.22.1, SDK Build Tools 36.0.0, Platform Tools 37.0.1, plataformas 34/35/36/37 e Command Line Tools 16.0 instalados. A verificação oficial retornou sucesso e status ok para todos os 13 componentes. Java e adb executaram corretamente; source.properties confirmou a versão do NDK. Nenhum telefone foi encontrado por adb devices.

O suporte Android foi instalado primeiro, com sucesso. A chamada seguinte para instalar os filhos também tentou repetir o módulo pai e reportou falha nessa repetição; todos os 11 filhos foram instalados. A verificação posterior confirmou o pai e os filhos presentes, sem erro. Esse resultado não substitui compilação de APK.

Referência de integração: https://code.visualstudio.com/docs/other/unity . O pacote com.unity.ide.visualstudio 2.0.25 atende à integração de VS Code; não representa instalação do Visual Studio IDE.

Tudo necessário para desenvolver e executar esta arena no Editor está disponível. Gradle pode baixar componentes na primeira compilação Android. Builds Windows IL2CPP exigirão ferramentas C++ específicas na etapa de builds. Agora há executável Windows e APK Android de desenvolvimento; consultar a validação adicional abaixo.

## Correção e validação no PC — 13/10/2026

O usuário abriu a cena vazia Untitled do template e tentou Play. Foi acrescentado ArenaLauncher: abre a arena na inicialização quando existe somente a cena padrão vazia e sem alterações; oferece F6/menu e redireciona Play da cena padrão vazia, preservando a confirmação de salvar cenas alteradas. Não substitui cenas com conteúdo próprio.

O módulo de áudio foi explicitamente incluído no manifest para impedir a remoção do AudioListener; física também passou a ser dependência direta. O teste de referências passou a exigir AudioListener na câmera. 11 testes de PlayMode foram executados novamente e todos passaram.

ArenaBuilds foi acrescentado para compilar a cena explícita: Windows x64/Mono e Android ARM64/IL2CPP. A versão Windows foi compilada com sucesso, executada e observada na tela. O usuário confirmou movimento WASD, ataque e reset funcionando. Executável local: Builds/Windows/ForjaDasMares.exe. Android: moto g15 detectado, API 35 e somente arm64-v8a; APK gerado e teste funcional concluído, como registrado abaixo.

Arquivos novos desta correção: README.md, Editor/ArenaLauncher.cs e Editor/ArenaBuilds.cs (com seus meta files). Alterados: Packages/manifest.json e lock, teste ArenaAcceptanceTests, configurações de plataforma necessárias, documentação de progresso. Nenhum sistema da próxima etapa foi iniciado.

## Backup remoto confirmado

Repositório: https://github.com/Fabrithizio/ForjaDasMares . O usuário autorizou explicitamente a publicação no repositório público após ser informado da visibilidade.

Checkpoint Windows validado enviado pela CLI Git à branch main: commit 20468aac96053e173fd6a8e5c734ac305140f675. Foi feita uma nova clonagem do GitHub, independente do checkout original: 155 arquivos e árvore e45ce78dee18226b6614d332299f8efcd9c440f4, exatamente iguais ao checkpoint local. Assets, meta files, Packages, ProjectSettings e documentação estão no backup. Caches, builds e metadados temporários de testes não estão incluídos.

A conexão GitHub do aplicativo recusou a primeira tentativa de escrita (HTTP 403, Resource not accessible by integration). O envio foi concluído pelo Git local com autenticação via Git Credential Manager. Nenhum segredo foi incluído no projeto.

Há também um ZIP local do código-fonte na entrega de validação. Builds executáveis ficam localmente em Builds; para recuperar o trabalho em outro PC, clonar o repositório e abrir com Unity 6000.3.26f1. Este registro descreve um checkpoint confirmado, não uma automação de backups futuros.

## Validação Android concluída — 13/10/2026

APK de desenvolvimento 0.1.0, pacote com.forjadasmares.prototype, IL2CPP/arm64-v8a, target API 35, mínimo API 25. O Gradle 9.3.1 do Editor retornou BUILD SUCCESSFUL para assembleDebug. APK instalado pelo adb com Success e aberto na atividade UnityPlayerActivity do moto g15 / Android 15. O app usa o perfil Mobile.

Teste real via entrada de toque injetada no aparelho conectado: joystick deslocou o personagem do spawn até o alvo; ataque reduziu a vida de 100 para 75; esquiva deslocou o personagem; reiniciar restaurou vida 100 e posição inicial. Capturas de cada estado foram conferidas. A consulta de logs filtrada pelo processo do jogo não encontrou Exception, NullReference, FATAL ou Error na sessão consultada. O jogo foi deixado aberto e reiniciado no celular.

Este é um teste funcional curto em um aparelho, não um benchmark, teste de longa duração, teste humano de ergonomia ou comprovação de compatibilidade com todos os Androids. Não há multiplayer/crossplay nesta entrega.

### Limitação de montagem Android nesta sessão

O pipeline Unity concluiu shaders e os 544 objetos nativos ARM64, mas sua invocação interna do Gradle falhou no Java NIO com Unable to establish loopback connection / UnixDomainSockets Invalid argument: connect. Foram ensaiadas preferências IPv4, seletor alternativo e opções de ambiente da Unity; as tentativas completas pela Unity ainda falharam no pós-processamento, sem erro C#.

A montagem real foi concluída executando diretamente o Gradle no projeto Android gerado em Library/Bee/Android/Prj/IL2CPP/Gradle. Um teste Java independente confirmou Selector.open funcionando quando o JDK usa seu fallback TCP. A opção jdk.net.unixdomain.tmpdir foi direcionada somente nesse processo a um caminho indisponível para sockets Unix; a implementação do PipeImpl do JDK instalado trata a falha do listener e usa TCP. Nenhuma configuração global de Windows, segurança ou firewall foi alterada.

O APK testado foi copiado do launcher-debug.apk produzido com BUILD SUCCESSFUL. O menu Build Android prototype usa o pipeline padrão Unity; sua montagem integral precisa ser revalidada no Editor aberto normalmente fora desta sessão. Não foi mantida uma tentativa de contornar o problema por alteração do ambiente dentro do C#, pois ela não se propagou à invocação Java da Unity.

Avisos observados: metadados SDK em Program Files não puderam ser regravados, e opções geradas pela Unity foram marcadas como deprecated pelo Gradle. A montagem direta terminou com sucesso apesar desses avisos. Reavaliar SDK gravável/atualizações na próxima preparação de builds, sem alterar permissões de segurança do sistema.

### Arquivos e builds

Criados nesta rodada: README.md, ArenaLauncher.cs e ArenaBuilds.cs com meta files, checkpoint Git e ZIP do projeto. Atualizados: manifest/lock para áudio/física, teste de referências, configurações Android ARM64/IL2CPP/API35, guia, progresso e auditoria. O helper de build inclui menus Windows/Android; a ferramenta de abertura oferece F6 e protege cenas alteradas.

Windows: Builds/Windows/ForjaDasMares.exe, compilado e confirmado pelo usuário. Android: Builds/Android/ForjaDasMares.apk, gerado, instalado e testado. Builds e capturas não ficam no Git; os arquivos que permitem reconstruir o projeto ficam no repositório. O novo checkpoint deve incluir estas conclusões.

Verificação final após os ajustes dos menus e da proteção de cenas: 11 testes PlayMode executados novamente, 11 aprovados, zero falhas; código C# compilado. O helper de abertura também preserva cenas com componentes personalizados. Nenhuma próxima etapa foi iniciada.


## Builds da etapa 3

Windows 0.2.0 x64/Mono: BuildPipeline retornou ARENA_PLAYER_BUILD_OK e saída 0. Executável iniciado nesta rodada; inicializou PhysX/Input System e carregou a cena, sem Exception, NullReference, MissingReference ou FATAL no log consultado. Encerrado o processo de teste criado por esta sessão. Não foi uma nova confirmação humana de teclado/mouse no executável; os comandos foram verificados nos testes PlayMode.

Captura real URP da arena atual gerada por ArenaPreviewCapture e inspecionada: chão/obstáculos/iluminação/câmera, guerreiro com espada geométrica, grupo de alvos, aliado e HUD presentes. Arquivo arena-herois.png na entrega. ZIP do executável e todos os dados Windows produzido.

Android 0.2.0: montagem em andamento neste checkpoint. Nova confirmação física depende do aparelho voltar a ser detectado por ADB. O código funcional no PC pode ser salvo no GitHub conforme autorização anterior.

### Nova rodada no aparelho

O moto g15 voltou a ser detectado. A primeira montagem 0.2.0 terminou no Gradle em 45 s e instalou com Success, mas o teste físico revelou stripping de SphereCollider chamado por GameObject.CreatePrimitive no pool. Este APK inicial não é a entrega final. Pool corrigido para criar MeshFilter/MeshRenderer com malhas explícitas previamente serializadas na cena, sem criar/remover colliders. Verificações e montagem estão sendo repetidas.

## Conclusão da etapa 3 — versão 0.2.0

Última suíte: 23 testes PlayMode aprovados, zero falhas, após a correção do pool Android e do reset do combo/trava de direção. O pool usa malhas serializadas com MeshFilter/MeshRenderer e não cria colliders no runtime. O stripping identificado no primeiro APK foi corrigido.

Android final: IL2CPP/ARM64, mínimo API 25, alvo API 35, versionCode 2, versionName 0.2.0. Montagem direta Gradle concluída em 22 s com BUILD SUCCESSFUL; instalada com Success e aberta no moto g15. Nova verificação da versão final: joystick segurado desloca o herói, primeiro ataque deixa alvo em 74, reset restaura alvo e reinicia combo (primeiro golpe novamente 74). Log de 52 linhas do novo processo, sem E Unity, Exception, NullReference, MissingReference ou FATAL na consulta. Perfil Mobile confirmado pelo diagnóstico de runtime. Capturas reais conferidas.

Sessão física desta etapa, com toque injetado no aparelho: Guerreiro 100→74→29 (golpe e impacto/empurrão de grupo); Patrulheiro projétil 100→80 e armadilha 100→72 com alvos imobilizados; Arcanista 100→84→64→46 (gelo/fogo/raio), pulso 100→68 com imobilização de grupo; Cavaleiro golpe+investida 100→54, proteção reduz dano no aliado 150→134 e no herói 210→196, depois expira (segundo dano: aliado 94, herói 163). Esquiva desloca o personagem e reset restaura arena/vida/câmera. Cada resultado observado em captura, além dos testes automatizados.

A proteção só começa após a preparação da habilidade; pressionar DANO imediatamente, antes da resolução, aplica o dano normal. Foi repetido o teste aguardando a preparação, comprovando redução e expiração. Não é invulnerabilidade permanente.

Limitações: placeholders geométricos e poses procedurais; gamepad físico, ergonomia humana/multitoque simultâneo, profiling, outros aparelhos e testes de longa duração ainda pendentes. A chamada interna do Gradle pela Unity continua falhando nesta máquina em Java NIO; o APK entregue usa montagem direta do projeto gerado, com ajuste somente no processo Java. Sem alteração de segurança/firewall/global do Windows. Não há equipamento, progressão, IA de inimigos ou multiplayer/crossplay nesta etapa.

O projeto permanece em Área de Trabalho/Jogos/ForjaDasMares. Abrir a cena por F6 e Play. A próxima etapa exige nova instrução do usuário.

SHA256 do APK entregue: 0493543A863A925F13627778B15D9D01FC9DD36F2616B71E899A72A9CAD3E092

Windows final recompilado com ARENA_PLAYER_BUILD_OK e saída 0. Executável final iniciado, ARENA_RUNTIME_READY confirmou PC_RPAsset e referências da arena; consulta do log sem exceções. Processo de teste encerrado e ZIP atualizado. Backup final inclui a correção Android e os 23 testes.

## Etapa 4 — integração PvE em validação final (13/10/2026)

Arena preservada; cinco arquétipos, minion de invocação e chefe Custódio da Forja com três ataques/fase 2. AI Navigation 2.0.16 e módulo AI resolvidos e compilados; dois NavMeshData salvos, raio do chefe 0,8. IA separada de movimento, dano e apresentação; CharacterController mantém colisões. Pool fixo de 24 atores, limite 12 em Android e 18 em PC, limite de invocação por dono e orçamento de sessão. Armas/ataques/vida/controle existentes reutilizados. Avisos geométricos e três bipes sintetizados originais, sem arte/clips finais.

HUD permite escolher encontro e iniciar. R cancela e limpa; troca de herói cancela o desafio da arena. Ondas aguardam a fila de spawn e os inimigos relevantes; derrota encerra o encontro. Chefe dá 25 selos temporários uma vez, sem inventário ou salvamento. Projetil termina no primeiro impacto; dano em área deduplica Health; morte limpa invocações, projéteis e avisos.

Primeira suíte: 33/33 PlayMode aprovados, incluindo os 23 anteriores, quatro heróis × cinco arquétipos e quatro heróis contra o chefe, morte, navegação ao redor da parede, bloqueio, áudio configurado, dano e limpeza. Duas verificações extras de ondas e colliders múltiplos foram acrescentadas e aguardam execução final. Build PC e captura ainda em andamento neste registro.

Android sem aparelho nesta etapa a pedido do usuário. Nenhuma confirmação humana, teste físico de celular, profiling de desempenho ou balanceamento comercial é alegado. Etapa 5 está autorizada, mas só começa após finalizar a validação da etapa 4. Multiplayer/equipamentos continuam fora do escopo.

## Fechamento PvE

35/35 testes PlayMode aprovados, zero falhas. As duas verificações adicionais confirmaram exatamente nove spawns nas três ondas e um único dano em área para o jogador com colliders múltiplos. O executável Windows 0.3.0 compilou, abriu com ARENA_RUNTIME_READY e PC_RPAsset, sem exceções no log. Captura URP real com três inimigos ativos revisada; a sobreposição do texto do encontro foi corrigida no HUD. Build final será recompilado com esse ajuste visual.

Etapa 4 validada no PC. Não houve teste físico Android nesta etapa; o telefone foi levado pelo usuário. Não há multiplayer, inventário ou arte final. Etapa 5 pode começar conforme autorização explícita de executar na ordem, sem confirmação presencial.

## Etapa 5 — masmorras em verificação final

Reutilizada a arena/PvE validada no checkpoint 7869a62. Nova DungeonTest, sem regravar ArenaTest: SHA256 da referência e da cena preservada 07D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4. Três biomas geométricos originais com materiais/luzes/decoração e tabelas diferentes; oito categorias de sala; layout seeded com percurso conectado e ramificações, 7–12 salas; modo fixo.

Portas físicas/carving fecham em encontro e abrem após vitória. Evento exige cristal; armadilha tem aviso/bipe e alavanca; chave da sessão abre tesouro opcional; baú abre e concede moedas uma vez; descanso cura uma vez; saída exige chefe vencido. Câmara com limite opcional usado somente na masmorra. NavMesh é construído e validado para inimigos normais e chefe; spawn valida geometria/colisão. Sem aumento arbitrário de atributos, equipamentos ou multiplayer.

Os testes individuais já passaram para 2.000 seeds × seis limites (12.000 combinações), mesma seed igual assinatura, 12 gerações reais em três biomas/quatro seeds, portas, objetivos, baú, reset e percurso completo com movimento/colisões/combate real do Cavaleiro Rúnico, descanso, três encontros, chefe e saída. Corrigido reset de alvos de referência inativos. Bot de teste usa NavMeshQueryFilter explícito do agente normal para evitar consultas ambíguas entre duas superfícies. A suíte completa e builds ainda estão em execução.

Android sem teste físico por instrução do usuário; preparar build para instalação posterior. Arte/clips/profiling/gamepad físico permanecem pendentes. A próxima etapa não será iniciada automaticamente.

Suíte completa da etapa 5: 41/41 PlayMode aprovados, zero falhas; mantém os 35 testes do PvE e acrescenta seis testes de masmorra. Percurso completo passou em 72,98 s no teste isolado e novamente na suíte completa. Seeds verificadas em 12.000 combinações lógicas; 12 gerações físicas com NavMesh de ambos os tamanhos. Builds/capturas seguem como validação separada, sem alegar Android físico.

Verificação adicional: câmera visível em 16:9,4:3 e9:16. A interação F foi exercitada pelo estado Dynamic do Input System, callback real e Update da sessão. O teste sem Game View focada precisou encaminhar o teclado sintético para o player; configurações de foco do teste são restauradas no finally e não alteram a configuração do jogo. Ajustado o raio da investida do chefe de0,9 para1,5 para superar a margem de contato dos colliders; aviso de linha agora cobre também a extensão final do golpe, e dano fica no semicírculo frontal. Suíte final de44 casos em execução, com caso novo para impacto único da investida.

Validação final do código: 44/44 testes PlayMode aprovados, zero falhas, incluindo a investida com dano real e único, F pelo Input System e câmera em três proporções. Os quatro heróis continuam vencendo cada arquétipo e o chefe com a correção de alcance. Percurso completo da masmorra aprovado novamente. Unity encerrou com resultado Passed. Capturas catacumbas/floresta revisadas; fortaleza/builds ainda em preparação.

Revisão final da interface: barra do chefe agora usa geometria real com largura proporcional à vida, pois Image Filled sem sprite não reduz o desenho. Helper reutilizado na arena e na masmorra, sem regravar ArenaTest. Teste verifica largura real, alinhamento e proporção. Suíte completa final: 45/45 aprovados, zero falhas. Windows0.4 compilou e abriu com DUNGEON_GENERATED/DUNGEON_RUNTIME_READY, sem exceções; recompilar para incluir o helper final. Android IL2CPP/URP compilou e gerou projeto Gradle, mas montagem interna encontrou Java NIO Unable to establish loopback connection, mesma limitação conhecida. Preparar montagem direta após recompilação final.

## Entrega final da etapa 5 — 13/10/2026

45/45 PlayMode aprovados. Windows0.4.0 final recompilado com o helper da barra, iniciado e DUNGEON_RUNTIME_READY/seed12345 confirmou geração de9 salas; log sem exceções. Processo de teste encerrado e ZIP atualizado. Atalho Jogar ForjaDasMares na pasta Área de Trabalho/Jogos aponta para Builds/Windows/ForjaDasMares.exe.

Android0.4.0 final: IL2CPP/URP/nativo gerados pela Unity; montagem interna falhou somente no problema conhecido Java NIO. Gradle direto terminou BUILD SUCCESSFUL em25s. APK copiado do launcher-debug.apk gerado, versãoCode4, minSDK25,targetSDK35,ARM64; assinatura v2 verificada pelo apksigner. Arquivo38707750 bytes. SHA25663803B4EFFFB7F1D8C8B4ABE206687EE0A14D23F4B84233614B8B88D7AC9614A. Esta versão não foi instalada/testada em aparelho: usuário levou o telefone. A compilação não prova funcionamento físico/crossplay.

Ajuste de Java limitado ao processo de montagem, sem alterações de firewall/segurança/ambiente global. Avisos de metadados SDK somente leitura e de opções Gradle obsoletas não impediram a montagem. O menu Android usa o pipeline padrão; nesta máquina a montagem direta permanece necessária nesta sessão.

Capturas URP reais dos três biomas revistas. ArenaTest preservada byte a byte, SHA25607D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4. Cenas/materiais/ScriptableObjects/prefabs/.meta/configurações estão no Git; Library/Builds ficam fora. Arquivo fonte ZIP e guias serão exportados do checkpoint final, com verificação em clone independente.

Limites: placeholders geométricos e poses, sem arte/clips finais; sem profiling de aparelhos, ergonomia humana, gamepad físico, equipamento completo, persistência completa, multiplayer/crossplay. Etapas4 e5 concluídas neste recorte; não avançar a equipamentos/rede sem próxima instrução. Desenvolvimento continua por blocos, com backup no GitHub após cada validação.

Backup concluído e verificado: checkpoint032e5e3a51f625e23282a21d9e0ee2b0d806d2d1 e árvore595b100317c2318b7ff7251041afa0497f417796 iguais em clone independente do GitHub e no projeto local. FonteZIP exportada por git archive. Entrega em outputs/ForjaDasMares-Etapa5: PCZIP,APK,fonteZIP,guia,progresso,verificação,recuperação,lista exata de105 arquivos criados/15 alterados desde o PvE (inclui.meta),XML45 e três capturas reais. Apenas documentos mudam após esse checkpoint para registrar a conferência; os binários e código testados permanecem os da validação45.

## Correção PC 0.4.1 — 13/10/2026

O teste humano revelou cenário preto e Development Console repetindo falta de recursos ScreenSpaceAmbientOcclusion. O registro anterior de inicialização do Windows 0.4.0 não comprovava a renderização sustentada do executável; as capturas eram do Editor e os 45 testes de lógica não detectaram este defeito gráfico. A validação anterior do executável fica corrigida por este registro. Não usar o ZIP de PC 0.4.0 para jogar.

Auditado o código instalado da URP 17.3: CoreBuildData e URPPreprocessBuild coletam recursos pelo EditorUserBuildSettings.activeBuildTarget; os strippers de SSAO dependem desses recursos. ArenaBuilds definia somente BuildPlayerOptions.target, permitindo gerar Windows após Android sem garantir o destino ativo. Agora o menu ativa o destino; em batch uma plataforma divergente interrompe a compilação com instrução explícita. Nova build iniciada com -buildTarget Win64, confirmação ARENA_BUILD_TARGET target=StandaloneWindows64 active=StandaloneWindows64 e ARENA_PLAYER_BUILD_OK, sem erros de compilação. SSAO continua ativo: a falha foi corrigida na geração dos recursos, sem esconder o console ou retirar efeitos para mascará-la.

Executável 0.4.1 observado diretamente na tela deste PC: catacumbas, floresta subterrânea e fortaleza vulcânica renderizadas; transição para ArenaTest; seleção de patrulheiro pelo botão; disparo de mouse reduziu alvo de 100 para 80; três inimigos perseguiram e causaram dano; reinício por botão restaurou vida e cancelou o encontro. Execução acompanhada por vários minutos, sem repetição SSAO ou exceções/LogError no log. A injeção de uma tecla pela ferramenta de controle não confirmou seleção por teclado; não alegamos um novo teste humano de WASD nesta correção. Os 45 testes anteriores não foram reexecutados nesta rodada: código runtime/cenas não foi alterado. ArenaTest conserva SHA256 07D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4.

Alterados somente ArenaBuilds.cs, ProjectSettings.asset (versão 0.4.1), README.md e este documento. PC ZIP atualizado em outputs/ForjaDasMares-Correcao-PC-0.4.1; atalho da pasta Jogos utiliza o executável corrigido. APK anterior continua 0.4.0 e não foi recompilado ou testado em aparelho nesta correção. Próxima compilação Android deve iniciar com -buildTarget Android. Nenhuma nova etapa de conteúdo foi iniciada.

## Etapa 6 — código de progressão em integração, 14/10/2026

Checkpoint intermediário, não entrega final: definidas instâncias de equipamento, raridades/afixos, fórmulas, regras de slots, serviço de perfil, save local e ponte de eventos. Ainda faltam catálogo real, cenas de composição, inventário/interface e validação integrada. A ordem autorizada é progressão → rede → interface completa → conteúdo → revisão de lançamento, com revisão técnica antes de avançar. O usuário autorizou continuar sem presença e fazer backup por bloco.

Auditoria inicial: sem nomes de scripts duplicados, métodos vazios ou TODO/FIXME críticos no código próprio. Runtime não importa UnityEditor; os três asmdefs próprios não formam ciclo. MeleeAttack/CombatFeedback antigos continuam referenciados pelo ArenaSession e instalador da base, mas ficam desativados nas cenas de heróis, preservando compatibilidade. Não apagar esses arquivos sem migrar referências. Rede/Lobby/Relay não existem no projeto; não alegar que estão integrados.

45/45 testes existentes passaram novamente com as primeiras extensões opcionais do combate. Os12 testes novos de domínio da progressão encontraram5 falhas: JsonUtility materializava campos nulos de classe como objetos vazios, impedindo equipagem e fazendo saves concluídos recuarem ao backup. Correção: clone preserva presença, envelope2 registra presença de expedição/resultado e envelope1 normaliza somente sentinelas vazias. Nova execução12/12 aprovada. Testes incluem escrita bloqueada com rollback, recuperação de backup/corrupção, migração e rejeição de versão futura, afixos, regras de secundária, comparação de duas mãos, descarte confirmado, estoque cheio e deduplicação após reload. Esses testes usam catálogo temporário; não comprovam inventário jogável nem reinício humano do executável.

Outras correções comprovadas nesta revisão: enum/NaN inválidos rejeitados pelo catálogo; removida dependência do validador de dados no cálculo de gameplay; comparação de arma de duas mãos retira bônus secundário; inimigo continua concedendo XP quando o saque temporário opcional está cheio; reserva para recompensas obrigatórias; OnDestroy da ponte ignora inicialização incompleta. ArenaTest e DungeonTest permanecem sem regravação. Compilação Unity disponível, não substituída por inspeção de texto. Prosseguir com integração da etapa6 antes da rede, conforme última autorização.

## Etapa6 — integração e PC validados, Android em preparação

Catálogo real com12 itens iniciais,9 categorias de armas,5 raridades,9 afixos limitados e tabelas de recompensa. Variantes ProgressionDungeon/ProgressionArena integram inventário, comparação, slots, entregas, descarte confirmado, resultado, opções e mascote. Equipamentos alteram os padrões reais; crítico/elemental/multiplicadores são capturados na ação e propagados a projéteis/zonas. Arma equipada acompanha as poses existentes; arte/animações continuam geométricas.

Suíte completa:68/68 PlayMode aprovados,0 falhas. Inclui todos45 anteriores,13 de domínio,9 de integração e percurso completo com equipamento/baú/chefe/saída/recompensas salvas. Auditoria de4 cenas/7 prefabs/92 definições/416 behaviours:0 scripts ausentes,0 referências obrigatórias/quebradas,0 problemas. A inspeção do executável encontrou rótulos estáticos vazios em botões; corrigidos e testados:9/9 integração repetida com assert dos textos,0 falhas.

Windows0.5.0 final compilado com destino ativo correto e ARENA_PLAYER_BUILD_OK. Executável observado diretamente: mapa, HUD, Lume, inventário paginado, textos dos botões, comparação e EQUIPAR funcionando. Nenhuma exceção/LogError/falta SSAO no log acompanhado. Probe em processos Windows separados: write abriu baú real, aplicou morte e gravou13 itens/5XP/5 moedas/armadura/opções; read após fechar/reabrir restaurou os mesmos dados, vida máxima160 e opções. Diretório de teste isolado; perfil normal do usuário preservado.

Build Android anterior foi cancelado para incluir o rótulo corrigido; versão final IL2CPP/ARM64 está compilando. Nenhum aparelho conectado em adb devices; teste físico/crossplay não realizado. Ainda não instalar pacotes de rede até finalizar este bloco. Etapas de rede/interface completa/conteúdo/lançamento permanecem na fila, por autorização do usuário de continuar enquanto dorme. Fonte intermediária anterior no GitHub79b21a8; novo backup após este bloco.

## Etapa6 — entrega final local0.5.0

Windows final0.5.0 conferido com cenário, inventário, rótulos, comparação e EQUIPAR reais; nenhum erro registrado no acompanhamento. ZIP68142673 bytes, SHA2569BDF8FA86B10954D9F9D88549A958C99B4B45A9FB816EC52B6EA639ED1AC8E16. Probe de reinício da aplicação passou:13 itens/5XP/5 moedas/armadura/vida máxima160/opções restaurados de arquivo em processos separados. Perfil de QA isolado; dados normais do usuário preservados. Atalho da pasta Jogos continua apontando para o executável atualizado.

Android final0.5.0: código/nativo IL2CPP e projeto Gradle gerados. Montagem interna falhou no Java NIO Unable to establish loopback connection; Gradle direto com ajuste restrito ao processo terminou BUILD SUCCESSFUL em45s. APK copiado do launcher-debug.apk gerado; package com.forjadasmares.prototype, versionCode5/versionName0.5.0,minSDK25,targetSDK35,ARM64; assinatura v2 verificada. SHA25627F3B367ED451CC08C68F057D71C8065D90832391F4CBA4C33990A863529E89D. Não instalado/testado em aparelho: adb não encontrou telefone.

A compilação Android removeu temporariamente os dois registros SSAO da lista ativa de UniversalRenderPipelineGlobalSettings; restauração da configuração canônica inspecionada após o Editor encerrar. Build Windows sempre ativa Win64 antes de coletar recursos. Não remover esses registros do fonte compartilhado: fonte de PC precisa deles. Não esconder falhas por desativar o Development Console.

Backup GitHub do bloco de código/cenas/dados/docs:73dfcb6731e62e752af6c5c81d28da323087e924. Entrega em outputs/ForjaDasMares-Etapa6: PCZIP,APK,fonteZIP,XML68,XML9 da revisão de rótulos,auditoriaJSON,logs de persistência,guia/revisão/lista de arquivos. Documentação de entrega será atualizada no próximo commit, sem mudança de código testado. Classificação: aprovado com limitações para este protótipo local; sem aprovação de lançamento, economia online, crossplay ou teste físico Android. Próxima etapa autorizada: auditoria/migração de rede; ainda não há pacotes/serviços configurados neste checkpoint.

## Etapa7 — bloco de sessão privada validado, implementação continua
Base72/72 e cena/protocolo6/6 PlayMode aprovados. Build Windows cooperativo separado gerado e executado:host+3 clientes, mesmo layout confirmado, quinto recusado, movimento/ataque remotos reais, morte do herói do host com equipe sobrevivente, saída/reconexão na mesma vaga e encerramento ao sair host. Logs da terceira execução sem exceções. Detalhes e limitações em REDE_SESSAO_VALIDACAO.md. Etapa7 ainda não concluída: faltam saques/progressão/reaparecimento completos, fluxo completo de PvE/chefe, medição/latência/QA visual. UGS implementado com guard de configuração, mas projeto não vinculado e internet/crossplay não testados. Interface completa e expansão seguem depois, sem pular etapas.

Revisão geral posterior ao bloco de sessão: suíte PlayMode completa78/78 aprovada,0 falhas, XML work/etapa7/regression-78.xml. Nenhuma compilação permanece em execução após essa suíte. A etapa7 continua parcial pelas limitações já documentadas.

Continuação do domínio de recompensas da etapa7: CoopRewardLedger e seis testes reais de persistência/deduplicação adicionados;6/6 aprovados em execução isolada. Ainda falta conectar o coordenador à sessão e replicar os perfis; nenhuma recompensa cooperativa integrada foi declarada pronta. Arquivos novos: Networking/CoopRewardLedger.cs e Networking/Tests/CoopRewardLedgerTests.cs, com metadados. Não houve mudança nas cenas, controles ou builds neste bloco.

Android físico0.5.0: moto g15 conectado, instalação-r preservando dados, movimento/ataque/habilidade/projétil/troca/inventário/equipagem e persistência após reinício observados no aparelho. Detalhes/limitações em ANDROID_TESTE_FISICO_2026-10-14.md. Rede PC–Android ainda não testada; preparação do APK cooperativo iniciada. Revisão do novo stick cooperativo encontrou pivot inferior esquerdo incompatível com cálculo central: ajustado para0.5,0.5 antes do build, sem alterar o stick local validado.

## Etapa7 — progressão integrada e LAN física, continuação14/10

PC e moto g15 conectados realmente2/4: mesmo mapa, movimento Android observado no host, início de encontro/sala2, vida/morte/derrota compartilhadas e Closed no celular ao encerrar o host. APK cooperativo inicial0.6.0 produzido/assinado/instalado. Esse primeiro teste não comprovou golpe remoto nem vitória: os heróis morreram enquanto ficaram sem lutar. Detalhes no documento Android. Internet/Relay continuam pendentes de vínculo UGS.

CoopProgression/CoopProfileChannel/CoopInventoryHud ligam perfis privados do host, saques reais, resultado e intents validados à sessão. Equipamentos aparecem nas replicas e modificam o combate da autoridade. GUID de reconexão persiste; morto reaparece após vitória de sala com50% de vida e ponto seguro; derrota/abandono finalizam os perfis. Health.Revive é explícito, Heal não ressuscita. HeroEquipment.Initialize remove assinaturas anteriores. Corrigido filtro de mouse da interface e reposicionados controles de sessão fora do joystick; revalidar na nova build física.

Suíte geral89/89 aprovada (zero falhas). Rede17/17 repetida após assertions de reload. Percurso automatizado da autoridade venceu combates/chefe/saída com ataques reais, concedeu/gravou resultado e rejeitou replay. Arquivos novos: três componentes CoopProgression/CoopProfileChannel/CoopInventoryHud e CoopProgressionTests, com.meta. Alterados: CoopActor/Director/Hud/Protocol/SmokeProbe/CoopInstaller/CoopProtocolTests, composição CoopDungeon, Health/HeroEquipment e documentação; versão de build será0.6.1. Compilação de builds Windows/Android em andamento; não afirmar ainda que esses binários novos foram testados fisicamente.

Windows cooperativo0.6.1 gerado com COOP_BUILD Succeeded/errors0. Quatro processos reais carregaram protocolo2/perfis privados; ataque remoto matou goblin e concedeu12XP a cada participante; baú real adicionou10XP, repetição retornouFalse sem crédito adicional. Derrota controlada pela instrumentação de QA finalizou cada perfil com13 itens/11XP. Após fechar/reabrir quatro processos, todos restauraram13 itens/11XP em Lobby; replicas receberam somente o perfil do seu próprio slot. Quinta conexão recusada; saída do host levou clientes aClosed/0 conectados. Provas em work/etapa7/four-final/proof. A derrota controlada e o baú acionado pelo harness não são uma vitória humana de quatro jogadores; o percurso com chefe continua comprovado pelo teste automatizado da autoridade.

Android0.6.1/code7 ainda em compilação nativa IL2CPP. Não declarar instalação nem teste físico desta revisão até produzir/verificar o APK. ArenaTest e DungeonTest conservam os SHA256 anteriores. Backup do código validado em Windows será feito agora; a validação física posterior será registrada em outro checkpoint.

### Continuação física concluída neste recorte

Backup do bloco validado:1b9f23f no GitHub. Android0.6.1/code7 real instalado e testado com Windows: ataque do telefone matou goblin, joystick moveu na autoridade, equipagem de adagas persistiu no host, reinício do cliente/reconexão em40s recuperou vaga/herói/vida/equipamento. Tentativa após90s foi recusada. Resultado de baú/derrota apareceu com item/XP/moedas reais no Android. Host rodou em segundo plano após ajuste de build. Reaparecimento0→70HP após vitória de sala observado no telefone; inimigos desse teste foram derrotados pelo harness, não por combate humano.

Correções finais: guardas no harness antes de usar mapa, apresentação do herói selecionado após Close e runInBackground na build Windows. O harness anterior gerou uma NullReferenceException ao receber comando no Lobby; nova rodada com a guarda não reproduziu.17/17 de rede aprovados novamente: network-guarded-17.xml. Fonte de gameplay offline preservado; suíte geral89/89 anterior permanece como regressão e a rede foi repetida nas correções finais. Android final das guardas foi compilado até projeto Gradle; empacotamento direto em andamento para entregar código e binário consistentes.

Classificação deste bloco: aprovado com limitações para cooperação privada LAN Windows–Android. Etapa7 global permanece parcial porque UGS/internet não está configurado/testado, faltam latência/sessão humana completa e ajustes de apresentação. Não avançar a expansão/conteúdo/lançamento como se esses requisitos estivessem prontos. Ver ANDROID_TESTE_FISICO_2026-10-14.md e REDE_PROGRESSAO.md. A etapa de interface completa ainda não foi implementada nesta entrega.

APK final das guardas montado com Gradle BUILD SUCCESSFUL29s, assinatura verificada, instalado-r no moto g15 e iniciado. SHA2569069C045C6B9AF3FFAED818EFABC0D293171C90D9A3CC321594C1DC8EC5A5F78, versão0.6.1/code7/min25/target35/ARM64. Windows final com guardas/background compilou Succeeded/errors0. Cliente final e host normal (sem-netProbe) conectados realmente2/4 em Lobby, sem exceção nos logs próprios examinados. Dados de QA ficaram em diretórios isolados; save offline anterior preservado. Criado atalho adicional Desktop/Jogos/Jogar ForjaDasMares Coop.lnk; atalho local anterior não mudou.

Pacote de entrega será exportado em outputs/ForjaDasMares-Etapa7-LAN com PCZIP/APK/fonteZIP/guias/XMLs, acompanhado de hashes. Não representa fechamento da etapa de internet nem aprovação de lançamento. Nenhum Editor ou build continua em execução; somente host e aplicativo do telefone estão abertos para teste humano, aguardando INICIAR PARTIDA.

### Complemento: teste com atraso e preparação para o usuário

Dois executáveis Windows0.6.1 testados por proxy UDP local com75ms por sentido, jitter±20ms e perda2%. Movimento/ataque remoto, XP12, baú22XP, repetição sem duplicação, derrota11XP e encerramentoClosed confirmados. Nenhuma Exception/Error nos dois logs. Detalhes, limites e reprodução em REDE_LATENCIA_2026-10-14.md; proxy em Tools/QA/udp_latency.py. Sem mudança de C#/cenas/pacotes/builds, sem nova compilação necessária. Saves de QA isolados. Não equivale a ping de internet nem quatro jogadores sob perda.

PC e moto g15 novamente observados em Lobby2/4 na sala normal para teste do usuário. A rodada ampla de bugs/mecânicas/melhorias ficou adiada conforme pedido; nenhuma etapa foi pulada. UGS sem vínculo continua impedindo concluir internet da etapa7. Classificação permanece aprovado com limitações para LAN, parcial para etapa7 global.

### Preparação de internet e publicação — 15/10/2026

CoopInternetSettings e Resources/CoopInternet.asset centralizam ambiente production, região automática e timeout45s. CoopCloud usa SetEnvironmentName, região Relay configurável e validação do ID vinculado; ausência de configuração mantém offline/LAN. Menu Forja das Mares/Internet/Configuração cria/seleciona o asset sem regravar cenas. A primeira compilação encontrou referência ausente à assembly Unity.Services.Core.Environments; adicionada ao asmdef. Segunda compilação e criação do asset concluídas pela Unity.

Rede PlayMode **20/20 aprovada,0 falhas**, XML work/etapa7/internet-network-tests.xml. Inclui17 anteriores e3 novos: configuração pública/defaults, rejeição de Project ID ausente/inválido e tentativa internet sem vínculo preservando offline/sem iniciar transporte. Esses testes não chamam UGS real. Nenhuma build Windows/Android nova foi produzida nesta rodada; executáveis0.6.1 continuam os anteriores e não contêm esta configuração nova.

Unity Cloud foi aberto para o titular e chegou à autenticação/verificação em duas etapas, aguardando ação pessoal. Nenhum projeto/serviço contratado/conta criada/segredo embutido. Internet permanece **pendente**, sem aprovação comercial. INTERNET_E_PUBLICACAO.md documenta vínculo, teste entre redes, limites host/Relay, pontos de migração reais para servidor dedicado e requisitos Play Store consultados (API36/AAB/16KB/assinatura). Servidor dedicado ainda requer extração do papel server-only, identidade validada e persistência backend; não é troca instantânea de endereço. ESTADO_DOS_PEDIDOS.md distingue os dez pedidos detalhados e registra UI/expansão/lançamento/revisão final pendentes.

Criados: CoopInternetSettings.cs, Editor/CoopInternetSetup.cs, Tests/CoopInternetTests.cs, Resources/CoopInternet.asset e metadados; Docs/INTERNET_E_PUBLICACAO.md e ESTADO_DOS_PEDIDOS.md. Alterados: CoopCloud.cs, Networking.asmdef, este progresso. Arena, controles, cenas, dados de combate e pacote de QA preservados. Próxima ação depende do login/vínculo real, seguida de build e teste Relay PC–Android; não declarar todos os pedidos concluídos.

### Unity Cloud real e reconexão no Relay — continuação15/10

Titular concluiu login; projeto separado ForjaDasMares criado e vinculado (Project ID d8fae52c-f0ba-4d51-8e2b-faf465d77c7a). Windows0.6.2 em InternetWindows compilou e dois processos reais entraram em sessão privada MPS/Lobby/Relay, mesmo mapa, movimento e morte por ataque remoto confirmados. Retorno inicial falhou porque Lobby ainda considerava o cliente membro; implementado cache de referência por projeto/ambiente/perfil SDK e ReconnectToSessionAsync, com fallback limitado e remoção do cache após Leave confirmado. Nenhuma credencial no fonte; sem contratação paga.

Windows corrigido recompilou Succeeded/errors0. Regressão21:20 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Reteste real: cliente encerrado abruptamente e reiniciado voltou à vaga1 com140HP/12XP, Playing2/4;59s entre as capturas pré-queda/pós-retorno. Baú concedeu22XP, repetiçãoFalse sem duplicação; derrota controlada sincronizou11XP/13 itens em cada perfil. Relatório REDE_INTERNET_2026-10-15.md distingue QA, serviço real e limites. Não representa sessão humana completa ou redes distintas.

Adaptações: CoopCloud, CoopSmokeProbe (flags explícitas de QA/perfis SDK separados), CoopInstaller (saídas vinculadas separadas0.6.2/code8), CoopInternetTests, ProjectSettings/UnityConnect e documentação. Primeira preparação Android cancelada para incluir a correção; build Android corrigida em andamento. Executáveis LAN0.6.1 anteriores preservados. Etapa permanece em validação PC–Android/Relay; nenhuma etapa posterior marcada pronta.

### Entrega física do Relay0.6.2 — 15/10/2026

Android IL2CPP/ARM64 gerado; Gradle interno falhou novamente no loopback Java NIO. Montagem direta do projeto recém-gerado com ajuste processual concluiu BUILD SUCCESSFUL1min. APK0.6.2/code8/min25/target35 assinado v2, SHA2568DB2A3C83649CF1E8089AB64AA6002774F1C215843F0B09625EE493E8417F481; instalado-r sem limpar dados. Editor encerrou; dois registros SSAO temporariamente retirados por Android restaurados ao fonte canônico. UnityConnectSettings retornou automaticamente ao estado anterior e não tem modificação persistida; o vínculo do runtime é demonstrado pelo Project ID e serviço real.

Windows e moto g15 conectados por código Relay real2/4: toque básico36→10HP, ataques seguintes mataram alvo e concederam12XP; joystick moveu posição real na autoridade. Após force-stop/reabrir, Android retornou ao mesmo host com140HP/12XP preservados em65s entre capturas. Baú administrativo22XP; derrota controlada mostrou11XP,6moedas e uma Lâmina do Cais incomum no aparelho. Saída do host foi reconhecida. Sem Exception/LogError nos registros próprios examinados. Provas e limites em REDE_INTERNET_2026-10-15.md. Usuário não tem dados móveis: não executado teste entre redes distintas.

Código desse bloco já no GitHub8026b42. Sem novos scripts runtime após os testes/builds: apenas documentação/entrega atualizada. README/COOP_COMO_TESTAR/ESTADO_DOS_PEDIDOS atualizados. Aprovado com limitações para protótipo privado Relay Windows–Android em Wi-Fi; não aprovado para Play Store/economia comercial. Interface completa, expansão e revisão de lançamento continuam pendentes. Atalho Jogar ForjaDasMares Online usa perfil InternetCoopProfiles separado; partida LAN antiga emPlaying foi mantida aberta. Nenhum servidor contratado ou migração de host implementada.

Sessões de QA encerradas. Versão InternetWindows normal aberta sem-netProbe, sala criada pelo botão real e Android conectado novamente; Lobby2/4 observado no PC. Atalho Online fica para teste humano, host deve iniciar a partida. Compilações encerradas; Unity não permanece trabalhando em segundo plano. Entrega em outputs/ForjaDasMares-Internet-0.6.2, com WindowsZIP,APK,fonteZIP,guias,XML e hashes; não equivale à build de publicação.

### Correção focada da câmera — 15/10/2026, versão0.6.3

Retomada após cancelamento: ArenaCamera substitui mudanças instantâneas de inclinação por velocidade limitada22°/s, retorno12°/s, teto75°, espera0.35s e extensão suave0.28s. Direção horizontal preservada; colisão recolhe imediatamente para não atravessar geometria. Snap continua somente para início/reset/teleporte. Quatro novos testes cobrem obstrução, retorno, proporções e pausa. Regressão97:96 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Windows compilou Succeeded/errors0; duas instâncias reais em perfis isolados confirmaram movimento, morte por ataque remoto e12XP. Sem exceções nos logs examinados.

Android nativo compilado; empacotamento interno falhou no Java loopback conhecido. Gradle direto concluiu BUILD SUCCESSFUL1min11s; APK0.6.3/code9 assinado, instalado-r e executado no moto g15. Sala local, movimento por toque e renderização observados; sem exceções no log próprio. A velocidade angular é comprovada pelos testes PlayMode; captura física não substitui avaliação em vídeo. Repetição de Relay não feita nesta revisão. Oclusão parcial por paredes e recolhimento imediato continuam como limites explícitos.

Criados CameraObstructionTests.cs/.meta e Docs/CAMERA_CORRECAO_2026-10-15.md. Alterados ArenaCamera.cs, CoopInstaller.cs, ProjectSettings.asset (versão/code), README, COOP_COMO_TESTAR e este progresso. Cenas/pacotes/saves preservados; SSAO canônico restaurado. Saídas InternetWindows-0.6.3 e InternetAndroid-0.6.3; atalho Online atualizado preservando InternetCoopProfiles. Jogos Windows antigos abertos continuam na versão anterior: reabrir o atalho aplica a câmera nova. Ver relatório para evidências e limites; demais etapas continuam com o estado anterior.

### Câmera estável0.6.4 — revisão após observar a partida

Problema residual confirmado visualmente no Windows0.6.3: caixas ainda alteravam ângulo e distância, apesar do limite de velocidade angular. ArenaCamera preserva direção/braço para obstrução comum; novo CameraOcclusion oculta renderers no caminho do herói, preservando colliders/Renderer.enabled e restaurando estado original após liberação/desativação. Recolhimento de emergência apenas quando a lente realmente sobrepõe geometria. Ocultação binária da peça inteira; uma câmera ativa por cliente, sem split-screen. Parâmetros e limites em CAMERA_ESTAVEL_2026-10-15.md.

PlayMode99 casos:98 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Seis testes de câmera incluem distância/ângulo constantes, restauração, piso, colisão e pausa. Windows0.6.4 compilou Succeeded/errors0 e menu foi observado após abrir executável novo. Atalho Online atualizado; saves preservados. Teste humano de enquadramento disponível. Android0.6.4 em preparação; não declarar aparelho atualizado neste checkpoint.

Criados CameraOcclusion.cs/.meta e relatório CAMERA_ESTAVEL_2026-10-15.md. Alterados ArenaCamera.cs, CameraObstructionTests.cs, CoopInstaller.cs e documentação; versão de ProjectSettings acompanha build. Cenas, rede e combate não alterados. Revisão completa do jogo continua com as pendências anteriormente documentadas.

### Semitransparência solicitada pelo titular —0.6.5

Usuário aprovou o enquadramento estável de0.6.4 e pediu manter os obstáculos parcialmente visíveis. ArenaCamera expõe Obstruction Opacity padrão35%. CameraOcclusion usa materiais transparentes temporários/cache em vez de desligar desenho; mantém colliders e flags de visibilidade, restaura arrays originais e destrói cópias ao desativar/resetar. Template real Resources/CameraOcclusionTransparent.mat conserva variante URP/Lit transparente na build. Nenhuma alteração de combate/rede/cenas/saves.

Dois passes específicos7/7 aprovados,0 falhas, último com assert de material do chão. Não repetida suíte ampla anterior99 casos depois deste ajuste. Windows0.6.5 compilado Succeeded/errors0, atalho Online atualizado e menu aberto observado. Aviso de Firewall exige escolha pessoal do titular; QA visual de gameplay não foi declarada concluída neste checkpoint. Android0.6.4 cancelado para incluir pedido; Android0.6.5 em compilação. Telefone não aparece mais no ADB, portanto teste físico novo está pendente.

Arquivos: ArenaCamera.cs, CameraOcclusion.cs, CameraObstructionTests.cs, CoopInstaller.cs e documentação; novos Resources/CameraOcclusionTransparent.mat/.meta e CAMERA_TRANSPARENCIA_2026-10-15.md. Limitações de alpha blending/peça inteira/uma câmera e shaders futuros estão no relatório.


Fechamento0.6.5: fonte do efeito no GitHub72c1253. APK Android0.6.5/code11 real produzido após compilação nativa e Gradle direto BUILD SUCCESSFUL1min9s; assinatura v2 e badging verificados. SHA2569233B565C662B577276D879193CB033512B0358DB7C3CAAA0E46C1AFE5C1ADAA. Salvo em Builds/InternetAndroid-0.6.5, sem instalação/teste físico porque ADB não encontra dispositivo. PC pronto/aberto, aviso Firewall exige ação pessoal para jogar; aparência final ainda depende de avaliação humana. Sem teste Relay novo. URP SSAO canônico restaurado e compiladores encerrados. Classificação aprovada com limitações para o ajuste de semitransparência. Outras etapas do projeto conservam o estado anterior.
