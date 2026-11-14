# Maga e guerreiro — animações e efeitos 0.17.0

## Entrega

- Sapphire Mage: 25 clips de ossos originais gerados no Blender para o esqueleto real fornecido. Idle com respiração, caminhada, corrida, esquiva, dano, morte, vitória, conjuração, habilidade, carregado e super. Variações próprias de cajado/grimório/foco mantêm a extensibilidade existente; foco não recebeu uma rodada visual específica.
- Cajado: três gestos de lançamento e carregado próprios; energia ciano, anéis nas mãos, pulso de liberação e rastro de projétil.
- Grimório: três gestos com mão livre mais ativa e sustentação do livro; carregado próprio, selos violetas geométricos e partículas. Projéteis e fogo do item preservam a cor real configurada no ataque. O selo funcional existente continua prendendo inimigos.
- Guerreiro: oito clips de espada/machado refinados com preparação, transferência de peso, pulso de golpe, punho e recuperação. Malha, locomoção e demais clips preservados.
- Espada: slash, retorno, estocada e pesado; cortes dourados e faíscas.
- Machado: corte, revés, giro e pesado; brilho laranja e faíscas. O terceiro golpe mantém a geometria circular real.
- Fúria continua sendo aumento temporário de cadência, preservando o sistema real; aura usa agora o material de energia transparente/aditivo. Não foi substituída por dano de área.
- Camadas de Animator separam locomoção e ações do tronco. BrasaFootPlant reutilizado na maga para apoio dos pés; CharacterController continua sendo autoridade de movimento. Sem root motion.
- Correção de escala 0.16.2 mantida. Não se alteraram dano, alcance, atributos, recompensas ou progressão.

## Efeitos e orçamento

HeroWeaponEffects: seis LineRenderers reaproveitados e um ParticleSystem de até 64 partículas por personagem, sem criação de GameObjects por ataque, luzes dinâmicas, corrotinas ou novos colisores. Emissão ligada à linha de tempo e ao ponto de impacto da apresentação; faíscas de acerto respondem aos hits reais da autoridade. Traços de corpo a corpo são limitados pelas paredes. Os efeitos decorativos não representam aumento de alcance ou de área. Os eventos essenciais de ação continuam chegando às réplicas; o efeito pontual de acerto não recebeu uma mensagem de rede nova.

HeroEnergy.shader cria núcleo e borda suave sem texturas externas; dois materiais compartilhados em Resources. Particle System é módulo oficial embutido 1.0.0, agora declarado no manifest. Rastros existentes dos projéteis reutilizados por host e réplicas receberam maior duração/largura; material da campanha atualizado. PrismaCastingView e demais efeitos funcionais foram preservados.

## Como testar

1. Abrir Builds/Atual/Windows/ForjaDasMaresCampanha.exe. Versão 0.17.0.
2. Em Personagem e equipamentos, selecionar Arcanista e equipar Cajado do Sal Azulado; depois comparar com Códice das Três Centelhas. Usar os itens existentes do perfil ou os drops normais; não foram concedidos itens ao perfil do usuário.
3. Entrar numa fase. WASD/setas, apontar com mouse, segurar ataque: sequência básica e carregado. E: habilidade. Espaço: esquiva. Q: super quando a barra estiver cheia.
4. Voltar ao menu e selecionar Guerreiro. Comparar espada Lâmina do Cais e Machado das Fendas; observar três golpes, carregado e Fúria.
5. Atacar andando e mudar direção; conferir pernas em movimento e tronco atacando. Conferir morte/reaparecimento e troca de equipamento. Avaliar especialmente mãos, punhos, capa, joelhos, ritmo e tamanho dos rastros.

## Reprodução dos assets

Blender 4.5: executar ArtSource/Characters/SapphireMage/animate_sapphire.py e ArtSource/Characters/Brasa/polish_weapon_actions.py. O primeiro gera SapphireAnimated.fbx e fonte .blend comprimida; SapphireMage.fbx original permanece intacto. O segundo mantém a malha/locomoção estáveis e refina as ações de espada/machado.

Na Unity: executar ForjaDasMares.Editor.HeroCombatPolishInstaller.Apply. Importa clips, configura estados/camadas/máscara, recursos de efeitos e grava a cena CampaignCoop. VerifyMotion é diagnóstico local de curvas e posições das mãos. Nenhuma credencial, download externo ou nova licença de animação foi usado. Crédito/licença da malha original permanece no SOURCE.md.

## Verificação e limites

Testes focados: PrismaTests, ArsenalTests e LocomotionTests. Correções durante a rodada: Particle System ausente no manifest; geração de poses parcialmente sobrescrita pela avaliação do Blender; teste de espada presumindo herói inicial antigo; culling do Animator impedindo atualização de transformações fora da visão. Clips verificados na Unity e poses de cajado/livro distintas.

Build PC e capturas reais com perfis separados de QA. Clips registrados em execução: StaffBolt/Return/Release/HeavyStaff, BookWord/Page/Seal/HeavyBook, SwordSlash/Return/Thrust/HeavySword e AxeCleave/Backhand/Spin/HeavyAxe. Capturas inicialmente pretas de janelas ocultas foram descartadas para avaliação visual; refeitas com janela visível. Não se alterou o save normal. Logs e capturas locais em Logs/HeroPolish, não publicados no Git.

Não houve teste físico Android, APK novo, sessão entre dispositivos ou medição de desempenho representativa nesta rodada. Novas ações usam o protocolo de apresentação existente; sincronização visual online desta versão ainda precisa de rodada real. FMOD reportou falha ao inicializar saída de áudio do PC de QA: som não foi validado. Isto é uma primeira entrega para avaliação artística do usuário, não promessa de animação final cinematográfica ou ausência total de clipping.

## Arquivos principais

ArtSource/Characters/SapphireMage; ArtSource/Characters/Brasa/polish_weapon_actions.py; SapphireAnimated.fbx/controller/mask/prefab; Brasa.fbx; SapphireMageInstaller.cs; HeroCombatPolishInstaller.cs; HeroWeaponEffects.cs; HeroEnergy.shader e materiais; HeroPresentation.cs; PooledProjectileTrail.cs; CampaignAbilityBar.cs; CoopSmokeProbe.cs; testes Prisma/Arsenal; CampaignCoop.unity; CampaignInstaller.cs; Packages/manifest.json e lock; ProjectSettings.asset e documentação.

Resultado final dos testes: 28/28 aprovados, zero falhas/ignorados, Logs/hero-polish-tests.xml. Inclui emissão visual após impacto, limpeza ao desativar e vida inalterada pelos efeitos.

Build final Windows 0.17.0: Succeeded/errors=0/exit=0, Logs/hero-polish-windows.log.
