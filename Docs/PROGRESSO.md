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

Não há multiplayer, progressão, inventário, inimigos ou conteúdo comercial nesta entrega. Os placeholders não são modelos/animações finais. Android ainda exige build e teste em aparelho real; não há APK entregue nem crossplay comprovado. Perfil de desempenho e ajustes de toque/safe area/acessibilidade completos precisam de testes em dispositivos. Não há áudio nesta arena. A câmera foi validada em obstáculos baixos desta cena; salas fechadas precisam de nova avaliação.

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

Tudo necessário para desenvolver e executar esta arena no Editor está disponível. Gradle pode baixar componentes na primeira compilação Android. Builds Windows IL2CPP exigirão ferramentas C++ específicas na etapa de builds. Não há APK ou executável standalone entregue nesta etapa.

## Correção e validação no PC — 13/10/2026

O usuário abriu a cena vazia Untitled do template e tentou Play. Foi acrescentado ArenaLauncher: abre a arena na inicialização quando existe somente a cena padrão vazia e sem alterações; oferece F6/menu e redireciona Play da cena padrão vazia, preservando a confirmação de salvar cenas alteradas. Não substitui cenas com conteúdo próprio.

O módulo de áudio foi explicitamente incluído no manifest para impedir a remoção do AudioListener; física também passou a ser dependência direta. O teste de referências passou a exigir AudioListener na câmera. 11 testes de PlayMode foram executados novamente e todos passaram.

ArenaBuilds foi acrescentado para compilar a cena explícita: Windows x64/Mono e Android ARM64/IL2CPP. A versão Windows foi compilada com sucesso, executada e observada na tela. O usuário confirmou movimento WASD, ataque e reset funcionando. Executável local: Builds/Windows/ForjaDasMares.exe. Android: moto g15 detectado, API 35 e somente arm64-v8a; a compilação está em andamento. O resultado real será registrado antes do encerramento.

Arquivos novos desta correção: README.md, Editor/ArenaLauncher.cs e Editor/ArenaBuilds.cs (com seus meta files). Alterados: Packages/manifest.json e lock, teste ArenaAcceptanceTests, configurações de plataforma necessárias, documentação de progresso. Nenhum sistema da próxima etapa foi iniciado.
