# HUD rústico — PC e Android

## Direção

Primeira apresentação medieval rústica, com verde escuro de floresta, cobre, pergaminho e vida em vermelho terroso. Painéis arredondados e ícones vetoriais originais de espada, runa, esquiva, mochila, elmo, interação, bússola e menu. Nenhuma imagem baixada ou dependência de licença externa adicionada. Fonte LegacyRuntime fornecida pela Unity. Tema definitivo continua aberto à direção do titular.

## Integração

`DungeonHud.Start` monta `RusticDungeonHud` sobre a Canvas existente e desativa somente o apresentador antigo. Reutiliza ações dos botões originais, objetos de toque, TouchMoveStick, OnScreenButton, vida, combate, esquiva, cooldowns, progressão e janelas reais de inventário. Não recria a base nem modifica dano/recompensas.

- Vida/XP/nível no canto superior esquerdo; nome de herói ajusta fonte quando longo.
- Local e objetivo contextual no topo, sem seed/sala/debug cobrindo o personagem.
- Mochila, troca de herói e menu no canto superior direito. Ouro permanente e ouro temporário distinguíveis pelo sinal+.
- Ferramentas de seed/bioma/fixo/reset e retorno à arena preservadas em menus, sem ocupar a vista de combate.
- PC: dock compacto de ataque, habilidade e esquiva com ações reais, atalhos e cooldowns; indicação contextual de interação/F.
- Android: joystick circular à esquerda; ataque maior, habilidade, esquiva e interação à direita, separados. Área de captura do joystick preservada; só o thumb se desloca. Botão de interação só habilita com alvo utilizável próximo.
- Safe area da tela aplicada ao HUD/controles. Canvas escala com resolução. Texto ampliado do perfil também afeta os novos rótulos.
- Inventário/equipamentos/resultado/opções e confirmação de descarte usam o mesmo tema arredondado, preservando sua lógica e navegação anteriores. Bloqueio de ataque sobre os novos botões registrado no ProgressionHud.
- Saque em mensagem temporária; chefe com barra própria quando ativo.

Janelas online têm Canvas/CoopHud próprios; esta entrega reorganiza o HUD local das masmorras, inclusive Galerias, no PC e Android. Não declarar o lobby online redesenhado. O aplicativo online instalado anteriormente permanece separado.

## Arquivos

Novos em Scripts/UI: RoundedHudGraphic.cs, HudIcon.cs, RusticHudTheme.cs e RusticDungeonHud.cs, com metas geradas pela Unity. Alterados DungeonHud.cs e ProgressionHud.cs. Novo teste RusticHudTests.cs/meta. Build Galerias0.6.7 também inclui a correção de câmera em CAMERA_CONTINUA_2026-10-15.md.

RoundedHudGraphic gera a malha arredondada com borda, sem texturas ou materiais clonados por atualização. HudIcon desenha ícones em malha UI. Quando já existe Image, a superfície nova fica num filho: Unity permite apenas um Graphic por objeto. As referências originais são preservadas e a imagem antiga é desativada.

## Validação

Resultados de compilação/testes/builds e capturas serão acrescentados ao concluir. Não afirmar avaliação humana, gamepad físico ou desempenho sustentado sem execução. Limite atual: janelas de inventário mantêm a organização em duas colunas; telas extremamente estreitas/retrato precisam de revisão própria, enquanto o jogo Android usa paisagem.

## Checkpoint executável PC

Primeira rodada17 casos:8 aprovados/9 falhas, por tentativa de adicionar dois Graphic ao mesmo objeto de botão. Corrigido com superfície filha e Image original preservada/desativada. Rodada seguinte17/17 aprovada,0 falhas (work/galerias/hud-fixed-tests.xml). Inclui8 testes de câmera,6 do mapa e3 de HUD. Suíte ampla107 anterior é da correção de câmera antes do HUD; não foi apresentada como repetida depois desta interface.

Build Windows0.6.7 real: GALERIAS_BUILD StandaloneWindows64 Succeeded/errors0. Executável aberto com perfil QA separado; captura mostra composição nova, mas aviso de Firewall cobre parte da tela e precisa de resposta pessoal. Sem confirmação de revisão visual completa neste checkpoint. Android em compilação. Janelas de inventário e controles de toque aguardam capturas da build final.

## Revisão física e refinamento0.6.8

APK0.6.7/code13/ARM64 assinado v2 instalado-r realmente no moto g15 e executado. HUD, menu, inventário e seleção/comparação de item observados por toques ADB; fechamento, joystick, semitransparência e habilidade com cooldown5s observados. Log próprio sem exceções Unity/falhas fatais examinadas. Evidências work/galerias/hud-android-main/menu/inventory2/compare/moved/combat.png e hud-android-067-device.log. Não é uma sessão humana completa.

Revisão visual encontrou preenchimento de XP com altura0 no trilho fino e sobreposição de controles sob menu. Corrigida altura mínima2 e controles móveis recolhidos durante menu/ferramentas, liberando o dedo capturado pelo joystick. Novo assert de altura no teste. Última rodada17/17 aprovada,0 falhas (hud-final-tests.xml). Builds finais avançam para0.6.8/code14 para distinguir o refinamento;0.6.7 fica apenas como checkpoint de QA. Salvamento do binário Windows0.6.7 também passou write/read em perfis isolados:13itens/5XP/5moedas/armadura/max160/opções.

## Como testar no PC e no celular

Abra o atalho Desktop/Jogos/Jogar Galerias da Mare.lnk (atualizado para0.6.8) ou a cena GaleriasDungeon. No celular, abra Forja das Mares - Galerias, aplicativo separado do online. Caminhe junto a caixas/paredes e atravesse o corredor: câmera deve manter ângulo e seguir o jogador. Toque em Mochila fora de combate, selecione um item para comparar atributos, feche e volte a mover. Abra Menu, teste Opções e Ferramentas de teste; no Android, os controles de combate se recolhem enquanto esse menu está aberto. Use habilidade/esquiva e confira segundos de cooldown. Texto ampliado fica na aba Opções existente. Não use descarte sem intenção: preservada confirmação explícita.

Windows final0.6.8 compilado Succeeded/errors0. Build real em Builds/GaleriasWindows-0.6.8/ForjaDasMaresGalerias.exe. Android final está em compilação neste checkpoint; o aparelho tem0.6.7 de QA até a instalação final. Aviso de Firewall requer ação pessoal; autorização do usuário não altera a proibição da ferramenta de atuar em permissões de segurança.

## Finalização0.6.8

Windows final compilado Succeeded/errors0. Binário0.6.8 executado em duas passagens headless isoladas: PROGRESSION_SMOKE_WRITE_OK e READ_OK,13 itens/5XP/5moedas, armadura/maxHealth160 e opções restauradas. Logs hud-final-smoke-write/read.log, saídas0. Revisão visual interativa no PC permanece limitada pelo aviso de Firewall da janela de QA0.6.7; não declarar revisão humana ou cliques nessa permissão. Atalho preserva perfil/argumentos e aponta0.6.8.

Android final teve compilação nativa concluída e falha do empacotador interno Java loopback (4 erros de empacotamento, sem erro C#). Gradle direto concluiu BUILD SUCCESSFUL45s. APK real0.6.8/code14/ARM64,50.804.539 bytes, assinatura v2 verificada. SHA256 F8D4AEC43D00F5EEA818DCE1021472EA502C0B6A3DAE6B82C18CCD7F6FE95695. Instalado-r e executado no moto g15; mesmo pacote separado com.forjadasmares.galerias, sem limpar dados ou alterar o aplicativo online.

Capturas físicas finais: hud-final-android-main.png confirma barra XP39/60 preenchida; menu.png confirma controles recolhidos; inventory.png confirma navegação por toque; controls.png confirma retomada de joystick e habilidade com efeito/cooldown5s. Arquivos no workspace work/galerias; log restrito ao PID hud-final-android-device.log confirma seed73129 e não mostra exceções Unity/falhas fatais examinadas. Não substitui sessão humana completa, gamepad físico, teste de todos os heróis/chefes, vídeo quadro a quadro ou profiling.

ProjectSettings retém identidade principal Arena/com.forjadasmares.prototype e versão0.6.8/code14; somente a build Galerias usa nome/pacote separado. RIDs SSAO canônicos restaurados após Android; cenas/pacotes/saves preservados. Classificação: aprovado com limitações para câmera/HUD local de masmorras. Lista completa das alterações de câmera e interface nos dois relatórios; lançamento, arte final, redesenho orgânico do mapa e HUD online continuam fora desta entrega.
