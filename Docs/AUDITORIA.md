# Auditoria inicial — etapa 1

Data: 12 de outubro de 2026, referência do usuário em America/Sao_Paulo.

## Escopo e evidências

Antes de escrever estes documentos foram inspecionadas as pastas `Desktop/Area Dev`, `Desktop/Jogos` e o workspace desta conversa. Foram procurados arquivos `ProjectSettings/ProjectVersion.txt` em Desktop e Documents. A busca não encontrou um projeto Unity nessas árvores. Isso não estabelece ausência em outros discos ou locais.

O usuário informou que o projeto ainda não existe e escolheu a pasta Jogos. Foi criada `C:/Users/Fabricio_S/Desktop/Jogos/ForjaDasMares`, inicialmente inexistente, com documentação. Não existiam arquivos equivalentes nessa pasta para reaproveitar.

| Item | Evidência e estado |
| --- | --- |
| Unity Hub | Registro de instalação: 3.22.2. Não equivale ao Unity Editor. |
| Unity Editor | Nenhum editor em `C:/Program Files/Unity/Hub/Editor`; nenhum processo Unity identificado; nenhum Editor identificado nos registros de desinstalação consultados. Não há versão de projeto confirmada. |
| Projeto e cenas | Nenhum projeto fornecido. Não há cenas, prefabs, scripts ou GUIDs para preservar. |
| Pacotes | Não há `Packages/manifest.json` ou `packages-lock.json` do jogo. Compatibilidade instalada não verificada. |
| Renderização | URP solicitada, ainda não configurada nem executada. |
| Input | Input System solicitado, ainda não instalado no jogo. |
| Rede e serviços | Nenhuma integração, projeto UGS vinculado, credencial, sala ou alocação Relay criada. |
| Builds e testes | Nenhum teste, compilação ou build executado nesta etapa. |

## Avaliação preliminar de dependências

A documentação oficial apresenta Netcode for GameObjects como solução para GameObject/MonoBehaviour e lista NGO 2.7.0 para Unity 6000.0. Isso não é confirmação de compatibilidade de uma futura versão 6000.x deste jogo; a versão exata dependerá do Editor escolhido e da resolução de pacotes.

Para Lobby e Relay, priorizar `com.unity.services.multiplayer` e a API de Sessions, avaliando também Authentication. O SDK unificado coordena os serviços. A documentação informa que o pacote Relay isolado está descontinuado para Unity 6. Não instalar APIs antigas e novas em paralelo sem uma razão verificada.

Unity Transport será a camada de transporte para NGO. No projeto real, conferir a dependência transitiva exigida pela versão do NGO; evitar sobrescrever sua versão arbitrariamente. URP e Input System também serão escolhidos na linha suportada pelo Editor.

Não foram fixadas versões em um manifest inventado. Não foi alegado crossplay, reconexão ou multiplayer funcionando.

## Procedimento para concluir a auditoria técnica

1. Instalar e licenciar uma versão Unity 6 LTS suportada pelo Hub, registrando o número completo do Editor. A indicação da versão final depende da disponibilidade no momento da instalação.
2. Criar pelo Editor/Hub um projeto com template Universal 3D dentro desta pasta, preservando `Docs` e `LEIA-ME.md`. Se o Hub exigir uma pasta vazia, usar uma pasta temporária e mover somente os arquivos do projeto para esta raiz; nunca sobrescrever a documentação existente.
3. Registrar `ProjectVersion.txt`, `manifest.json` e `packages-lock.json`, render pipeline ativo, cena inicial, configurações de Input e plataformas.
4. Conferir no Package Manager a resolução de URP e Input System. Reservar a instalação de NGO/Transport/MPS para a etapa 7; revalidar as versões imediatamente antes disso.
5. Abrir a cena inicial e conferir o Console para erros de compilação. Isso ainda não foi feito.
6. Só então fechar a auditoria técnica e iniciar a etapa 2 mediante a próxima instrução do usuário.

Android requer os módulos Android Build Support, SDK/NDK e OpenJDK correspondentes ao Editor. iOS requer iOS Build Support e um ambiente de build com macOS/Xcode ou Unity Build Automation. Ter Windows não permite produzir sozinho o aplicativo iOS final localmente. Não foram configuradas contas, assinatura ou publicação.

## Fontes oficiais consultadas

- [NGO no Unity 6000.0](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.netcode.gameobjects.html)
- [Sessions do Multiplayer Services SDK](https://docs.unity.com/en-us/mps-sdk/sessions-overview)
- [Relay com NGO e Multiplayer Services SDK](https://docs.unity.com/en-us/mps-sdk/tutorials/relay-and-ngo)
- [Configuração do ambiente iOS](https://docs.unity3d.com/6000.0/Documentation/Manual/ios-environment-setup.html)

Estas referências sustentam a escolha preliminar; não substituem importar, compilar e testar os pacotes e builds reais.

## Auditoria técnica concluída na etapa 2

Antes de implementar, esta pasta continha somente os quatro documentos da etapa 1. Não havia cena, script ou projeto Unity anterior a preservar. O template oficial Universal 3D 17.1.0 foi incorporado nesta mesma pasta, mantendo os documentos.

Editor instalado/licenciado: Unity 6.3 LTS 6000.3.26f1 (de8167e7440c). Manifest/lock resolvidos pelo UPM: URP/Core/ShaderGraph 17.3.0, Input System 1.14.2, uGUI 2.0.0, Test Framework 1.6.0 e integração do editor 2.0.25. O pacote de integração não instala o Visual Studio IDE; o editor de código configurado é VS Code.

A cena ArenaTest foi salva pelo Editor e incluída no build. Assemblies compilaram; 11 testes de PlayMode aprovados. Uma captura real em PlayMode foi renderizada e inspecionada. Na captura inicial, o batching produziu cores compartilhadas indevidas neste ambiente. GPU Resident Drawer e SRP Batcher foram desativados nos perfis PC/Mobile desta arena; a captura seguinte apresentou corretamente os materiais. Reavaliar essas otimizações com profiling e renderização real antes de reativá-las.

O log da primeira captura também registrou uma exceção interna de indexação UnityEditor.Search.SearchDatabase. Ela não foi causada por classes do jogo nem impediu a captura; verificar recorrência no Editor interativo. Houve diagnóstico de alocação persistente no encerramento de importação; não foi demonstrado vazamento do gameplay. Não há erro de compilação restante causado pelos scripts desta entrega.

Android: controles virtuais foram exercitados em testes de PlayMode com entrada sintética. Isso não comprova APK, desempenho em aparelho ou crossplay. O estado final de SDK/NDK/OpenJDK é registrado em PROGRESSO.md.

## Ambiente instalado e conferido

Unity Hub, Unity 6.3 LTS 6000.3.26f1, VS Code com C#/C# Dev Kit/Unity/.NET Install Tool e Git instalados. A solução do projeto foi gerada para o editor de código. Não foi instalado Visual Studio IDE.

Android Build Support, OpenJDK 17.0.18+8, NDK r27c (27.2.12479018), CMake 3.22.1, SDK Build Tools 36.0.0, Platform Tools 37.0.1, plataformas 34/35/36/37 e Command Line Tools 16.0 instalados. A verificação oficial retornou sucesso e status ok para todos os 13 componentes. Java e adb executaram corretamente; source.properties confirmou a versão do NDK. Nenhum telefone foi encontrado por adb devices.

O suporte Android foi instalado primeiro, com sucesso. A chamada seguinte para instalar os filhos também tentou repetir o módulo pai e reportou falha nessa repetição; todos os 11 filhos foram instalados. A verificação posterior confirmou o pai e os filhos presentes, sem erro. Esse resultado não substitui compilação de APK.

Referência de integração: https://code.visualstudio.com/docs/other/unity . O pacote com.unity.ide.visualstudio 2.0.25 atende à integração de VS Code; não representa instalação do Visual Studio IDE.

Tudo necessário para desenvolver e executar esta arena no Editor está disponível. Gradle pode baixar componentes na primeira compilação Android. Builds Windows IL2CPP exigirão ferramentas C++ específicas na etapa de builds. Não há APK ou executável standalone entregue nesta etapa.
