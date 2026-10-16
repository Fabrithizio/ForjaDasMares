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
