# Obstáculos semitransparentes — 0.6.5

Solicitação: manter a câmera estável de0.6.4 e deixar as peças encobrindo o herói parcialmente visíveis. Opacidade padrão35%, configurável no Inspector de ArenaCamera em Obstruction Opacity (15–75%). Sem alterar o braço/ângulo já validado ou desativar colisões.

CameraOcclusion substitui forceRenderingOff por cópias transparentes de material, armazenadas em cache por material original. Renderers retomam o array de materiais original após deixar de obstruir. Restore/Snap/desativação restauram materiais e destroem as cópias temporárias. Arrays por peça são criados no primeiro contato e reutilizados; casts têm buffer fixo. Renderer.enabled e forceRenderingOff anteriores permanecem intocados, inclusive geometria previamente ocultada por outro sistema. Pisos continuam fora da faixa de detecção.

Resources/CameraOcclusionTransparent.mat é um template real URP/Lit com alpha blending, ZWrite0 e passes de profundidade/sombra desativados. Sua inclusão em Resources conserva a variante transparente na build. A configuração foi comparada ao BaseShaderGUI.cs do pacote URP instalado746735fb06f2. As peças geométricas atuais usam URP/Lit e mantêm propriedades/cores dos materiais originais. Assets futuros com shaders, texturas/keywords ou efeitos especiais diferentes requerem revisão visual específica; não é suporte automático a todo shader existente.

Uma câmera de jogo ativa por cliente. Sobreposição de transparências tem os limites de ordenação próprios do alpha blending. O efeito vale para a peça inteira e muda imediatamente ao entrar em obstrução, com espera0.2s para restaurar. Não é transição de alpha animada. Apenas a posição real da lente dentro de geometria mantém recolhimento de emergência; caixas na linha de visão não acionam zoom.

Arquivos alterados: ArenaCamera.cs (opacidade), CameraOcclusion.cs (cache/restauração), CameraObstructionTests.cs (expectativa translúcida e teste de múltiplos slots/limpeza), CoopInstaller.cs (saídas0.6.5/code11), versão em ProjectSettings após build e documentação. Criados Resources/CameraOcclusionTransparent.mat/.meta e este relatório. Cenas/combate/rede/saves preservados.

Android0.6.4 em compilação foi cancelado para incluir o novo pedido. Nenhum APK dessa revisão cancelada foi apresentado como concluído. Telefone deixou de aparecer no ADB antes deste ajuste; teste físico novo depende de reconectar. Resultados de testes e builds abaixo quando finalizados.


## Checkpoint PC e testes

Sete testes de câmera aprovados,0 falhas, repetidos com verificação adicional de que o chão mantém o material original:7/7 novamente. XMLs work/camera-translucent/camera-tests.xml e camera-final.xml. Incluem múltiplos slots de material, alpha0.35, ZWrite0, física, restauração/limpeza, enquadramento estável, piso, aspecto e pausa. A suíte ampla99/98aprovados/1ignorado é da revisão0.6.4 anterior; não foi apresentada como repetida depois da mudança para transparência.

Windows0.6.5 compilou COOP_BUILD Succeeded/errors0. Atalho Online atualizado e executável novo aberto. Menu foi observado; o Windows exibiu aviso de Firewall sobre o novo caminho, que deve ser respondido pelo titular. A ferramenta de controle não opera avisos de segurança. Log inicial próprio examinado sem exceções. A avaliação visual da transparência em gameplay ainda depende de liberar esse aviso/entrar na sala; não é inferida somente pelo alpha do material em teste.


## Fechamento dos arquivos de desenvolvimento

Android0.6.5/code11/ARM64/min25/target35 compilado até projeto Gradle; empacotamento interno falhou no loopback Java conhecido, montagem direta do projeto recém-gerado concluiu BUILD SUCCESSFUL1min9s. APK verificado com aapt (versão/code/arquitetura) e apksigner (assinatura v2), copiado para Builds/InternetAndroid-0.6.5/ForjaDasMaresCoop.apk. SHA2569233B565C662B577276D879193CB033512B0358DB7C3CAAA0E46C1AFE5C1ADAA. ADB sem dispositivos: não instalado nem testado fisicamente. Não foi repetido Relay nesta revisão visual.

Dois registros SSAO retirados temporariamente pelo build Android foram restaurados após o Editor sair. ArenaTest e DungeonTest mantêm os hashes já registrados. Fonte do efeito no GitHub72c1253; este fechamento inclui versão/configuração e documentação. Nenhuma compilação Unity ou Gradle desta tarefa permanece em andamento. Jogo Windows0.6.5 mantido aberto para teste humano, se ainda não foi fechado pelo usuário.

Classificação: aprovado com limitações para implementação/compilação do efeito. Sete testes automatizados da câmera passaram; gameplay visual ainda precisa de avaliação humana após responder ao aviso Windows, e Android físico está pendente. Não alegar validação visual final somente por configurar alpha.

Lista final: alterados Scripts/Presentation/ArenaCamera.cs e CameraOcclusion.cs, Tests/PlayMode/CameraObstructionTests.cs, Networking/Editor/CoopInstaller.cs, ProjectSettings/ProjectSettings.asset, README.md, Docs/PROGRESSO.md e COOP_COMO_TESTAR.md. Criados Resources/CameraOcclusionTransparent.mat/.meta e Docs/CAMERA_TRANSPARENCIA_2026-10-15.md. Ajustes anteriores de câmera permanecem documentados em CAMERA_ESTAVEL_2026-10-15.md; nenhuma cena foi regravada.
