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
