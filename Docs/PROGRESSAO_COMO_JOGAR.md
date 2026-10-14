# Progressão local — como jogar

Cena inicial: `Assets/ForjaDasMares/Scenes/ProgressionDungeon.unity`; menu do Editor `Forja das Mares → Open progression dungeon`. `ProgressionArena` usa a arena existente com inventário e recompensas por encontro. As cenas anteriores `ArenaTest` e `DungeonTest` continuam como referência/testes sem progressão.

## Controles

WASD/setas para mover, mouse para mirar/atacar, E/clique direito para habilidade, Espaço para esquiva, F para interagir, Tab/1–4 para herói e R para reiniciar. I/gamepad Start abre inventário fora de encontros; Esc/gamepad B fecha. Botões da tela permitem as mesmas operações; no Android existem joystick e botões. Orientação móvel é paisagem. O jogo não congela quando se abre Opções.

## Inventário e equipamento

O perfil novo recebe12 itens comuns para experimentar as9 categorias de armas e os3 slots de proteção/suporte. Isso é conteúdo inicial de teste, não compra. Abra INVENTÁRIO/I, selecione o item, confira atributos/requisitos e EQUIPAR. SECUNDÁRIA aparece habilitada quando a regra permite. EQUIPAMENTOS permite selecionar um slot e DESEQUIPAR. DESCARTAR abre uma confirmação; item equipado deve ser desequipado de todos os heróis primeiro.

Espada/escudo: Guerreiro e Cavaleiro Rúnico. Machado: Guerreiro. Martelo: Guerreiro/Cavaleiro. Arco/adagas: Patrulheiro. Cajado/grimório/orbe: Arcanista. Cada classe guarda seu conjunto, usando o inventário compartilhado do perfil. Duas mãos retira a secundária do conjunto, guardando-a no inventário. O Patrulheiro pode usar outra instância de adaga na secundária com adaga principal. O mesmo item não pode ocupar dois slots no mesmo conjunto.

Inventário24 espaços, configurável no catálogo. Equipados ocupam espaço. ENTREGAS conserva recompensas quando o inventário está cheio; selecione e RECEBER após liberar espaço. SAQUE mostra equipamentos temporários da expedição, que ainda não podem ser equipados. Não há descarte automático em mudança de cena.

## Recompensas e resultado

Inimigos dão12 XP/2 moedas e18% de chance de item; invocações menores não dão recompensa, evitando geração infinita pelo invocador. Sala vencida dá15 XP/4 moedas; baú10 XP/10 moedas/1 item; chefe150 XP/30 moedas/2 itens no mínimo raros; saída após vencer o chefe90 XP/20 moedas/1 item no mínimo raro. Tabelas são configuráveis.

Vitória transfere100% do saque/XP/moedas ao perfil. Morte ou abandono guarda todos os itens encontrados e metade de XP/moedas. R, mudança de herói/bioma/seed e voltar à arena encerram o percurso atual; esses são mecanismos de teste. Ao reabrir a aplicação, expedição pendente é encerrada como interrompida, sem retomar o estado de combate. Concessões têm ledger por expedição e não podem ser repetidas com a mesma identificação. Uma expedição vazia abandonada preserva o último resultado relevante.

RESULTADO mostra recebimentos reais; o painel surge ao encerrar uma expedição com ganhos ou derrota. O nível é compartilhado por todas as classes e cresce depois de transferir XP ao perfil. Não existe loja, anúncio, dinheiro real ou economia online.

## Raridades e combate

Probabilidades gerais: comum55%, incomum30%, rara10%, épica4%, lendária1%. Chefe/saída sorteiam somente entre rara/épica/lendária com pesos10/4/1. Potência da arma e bônus base:1/1,08/1,16/1,25/1,35. Afixos únicos por item:0/1/2/3/3. Arma lendária gera um pulso próximo de6 de dano base e lentidão a cada três básicos; esse golpe adicional é intencional e deduplicado por alvo.

Espada tem combo/arco moderado; machado tem arco amplo; martelo faz impacto circular; arco lança flecha rápida; adagas fazem dois golpes; cajado lança projétil com área e lentidão; grimório dispara leque de três projéteis; orbe tem área/raiz curta e cadência baixa; escudo reduz dano frontal. Modelos e poses são geométricos provisórios. Não há clips/modelos finais.

## Mascote e opções

Lume segue o jogador sem collider, cura4 HP com cooldown10s, até12 vezes por expedição. Não revive nem bloqueia passagens; desligar/ligar não renova cooldown/cargas. Opções atuais: volume global, mascote e texto ampliado, gravadas no perfil. Controles móveis ajustáveis, volumes separados e menus completos pertencem à etapa de interface.

## Salvamento e recuperação

Arquivo `progress-v2.json` em `Application.persistentDataPath`. Windows atual: `%USERPROFILE%/AppData/LocalLow/ForjaDasMaresPrototype/Forja das Mares - Arena/`. `.bak` guarda a versão anterior; `.tmp` é usado durante escrita. Se o principal estiver inválido, o carregador tenta o backup e informa a recuperação. Se ambos falharem, preserva os arquivos e mostra erro em vez de zerar o perfil. Nunca envie senhas/tokens nesse arquivo. Esse perfil local é editável e não deve virar autoridade de uma futura economia online.

Testes usam diretórios temporários próprios. Probe de executável: `-progressionSavePath <pasta-de-teste> -progressionSmoke write` e depois `read`, disponíveis somente em build de desenvolvimento. Isso testa fechar/reabrir a aplicação com baú, morte, equipamento e opções reais, sem alterar seu perfil normal.
