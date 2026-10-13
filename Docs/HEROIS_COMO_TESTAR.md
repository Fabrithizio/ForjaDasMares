# Etapa 3 — heróis e combate

## Executar

Abra o projeto existente na Unity 6000.3.26f1. Use F6 ou o menu Forja das Mares/Open test arena e dê Play. A cena salva é Assets/ForjaDasMares/Scenes/ArenaTest.unity. Não é necessário gerar a arena novamente.

PC: WASD/setas movem, mouse aponta, clique esquerdo ataca, E ou clique direito usa habilidade, Espaço esquiva, 1–4 ou Tab troca herói, R reinicia. Gamepad: stick esquerdo, botão inferior para ataque, direito para esquiva, esquerdo para habilidade, ombro direito troca e Start reinicia. Gamepad físico ainda requer teste humano.

Android: segure e arraste o joystick fixo à esquerda. Somente o indicador interno se move. Use Ataque, Esquiva e Habilidade à direita. Troque o herói e reinicie pelos botões superiores. A direção dos golpes acompanha o último movimento no toque/gamepad. Sem movimento anterior, o personagem ataca à frente. A troca é uma ferramenta de teste: restaura vida/cooldowns e cancela os efeitos anteriores.

## Heróis provisórios originais

| Perfil | Vida | Velocidade | Ataque | Habilidade | Limitação |
| --- | --- | --- | --- | --- | --- |
| Guerreiro — Brasa | 140 | 5,8 | Espada em arco; combo 26/28/36 | Impacto 45 em raio 3,4; empurrão | Precisa aproximar-se |
| Patrulheiro — Vento | 105 | 6,8 | Flecha 20, alcance 13 | Armadilha 28; imobiliza por 1,5 s | Resistência baixa |
| Arcanista — Prisma | 80 | 5,5 | Gelo 16/lentidão, fogo 20/explosão, raio 18/empurrão | Pulso 32 em raio 4; imobiliza | Vida baixa |
| Cavaleiro Rúnico — Baluarte | 210 | 4,7 | Golpe 22; armadura 18% | Proteção 60% por 3 s e investida com colisão | Movimento lento |

Ataques têm preparação e recuperação. Pressionar durante a recuperação não dispara outro golpe. A cadeia avança por ataque; não há sistema de combos comerciais por timing nesta etapa. A habilidade tem cooldown de 5 s, exceto Arcanista, 7 s. A esquiva dura 0,22 s, com invulnerabilidade apenas entre 0,03 e 0,15 s. Patrulheiro tem esquiva mais rápida e cooldown menor.

Há três alvos inimigos imóveis para demonstrar dano de grupo, bloqueio, lentidão, imobilização e empurrão. O aliado azul perto do spawn permite demonstrar proteção. H ou DANO aplica 40 de dano de teste ao herói e ao aliado; use antes, durante e depois da proteção. V ou VITÓRIA apresenta a pose temporária. R restaura os alvos, posições, herói, aliado, efeitos e câmera.

## Dados e componentes

Assets/ForjaDasMares/Data/Heroes contém quatro HeroDefinition, ataques e habilidades separados. Vida, armadura, velocidade, esquiva, alcance, dano, preparação, recuperação, cooldown e efeitos são ajustáveis no Inspector. HeroRoster referencia a lista; HeroCombat executa os dados; DamageResolver valida direção, alcance, facção, paredes e duplicação; CombatEffectPool reutiliza 32 projéteis e 12 zonas. Capacidades são ajustáveis no objeto da cena. Não há alocação/instanciação de objetos a cada ataque após a criação do pool.

ArenaInput implementa IHeroCommands, sem unir entrada, câmera, movimento e combate. TouchMoveStick recebe eventos do EventSystem/Input System e envia vetor ao adaptador. Captura um dedo até soltá-lo e ignora o término de outro toque. Ataque/esquiva/habilidade continuam via botões virtuais do Input System.

Para um quinto herói: crie dados de ataque e habilidade, crie HeroDefinition, acrescente-o à lista HeroRoster e um modelo à lista de HeroPresentation, configurando visualSlot correspondente. Botão HERÓI/Tab passa por toda a lista; teclas 1–4 são atalhos dos quatro iniciais. Novos comportamentos de habilidade precisam estender AbilityEffect e sua resolução; heróis que reutilizam os efeitos existentes não exigem alteração de código.

## Apresentação e substituição

São cápsulas, cubos, cilindros e esferas, com espada, arco/aljava, bastão/foco e escudo para distinguir silhuetas. Não há modelos finais ou animações de esqueleto. HeroPresentation apresenta estados Idle, Walk, Attack, Skill, Dodge, Hit, Dead e Victory por movimento procedural do placeholder.

Para usar clips reais, substitua o modelo preservando a raiz visual e configure um Animator Controller com Speed (float), State (int conforme HeroState) e Alive (bool), referenciado no campo animator de HeroPresentation. Ajuste o componente para deixar a transformação do modelo ao Animator quando os clips assumirem a pose. O componente só escreve parâmetros quando há controller; nenhum controller vazio é declarado como animação pronta. O módulo Animation integrado à versão do Editor foi habilitado para essa integração.

## Escopo e limitações

Os alvos são bonecos de treino, sem IA. Proteção afeta o boneco aliado e o jogador local; não é multiplayer. Não há equipamento, inventário, progressão, salas, rede ou crossplay. O dano de teste e a troca de herói são recursos da arena de desenvolvimento. Ausência de dispositivo ADB impede validar o APK novo fisicamente nesta sessão; testes sintéticos de toque não substituem uso humano no aparelho.

O menu Upgrade existing arena with heroes é uma migração executada uma vez. Se já houver HeroRoster na cena, preserva dados e objetos. Não recria chão, câmera, obstáculos ou entrada. O ataque antigo fica desabilitado na cena e é conservado para as verificações de regressão da base.

As instruções de montagem Android e a limitação de Gradle desta máquina permanecem no guia da arena e no documento de progresso. A montagem usa o SDK/NDK/JDK da Unity, sem Visual Studio IDE.
