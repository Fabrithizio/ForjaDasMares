# Prisma — acabamento do Arcanista (0.15.0)

## Auditoria e escopo

A campanha já tinha Prisma, equipamentos de cajado/grimório/orbe, ataque mantido, pesado, esquiva, habilidade da arma e super. Faltava um modelo autoral com esqueleto/Animator: a classe ainda usava formas geométricas. Preservados arena, heróis, mapas, catálogo/IDs, progressão, saves v2, entradas e autoridade do host. Sem novos pacotes, serviços ou credenciais; protocolo 10 sem alteração de formato.

## Implementado

- Modelo 3D original editável no Blender: capuz aberto, túnica dividida, botas, manto com dois ossos, pergaminhos, emblema de cristal e cajado de reserva. 19 ossos, aproximadamente 3,7 mil triângulos. Visual estilizado de teste; não é arte final contratada.
- FBX, materiais URP, prefab, máscara de tronco e Animator com 25 clipes realmente exportados/keyframed. Estados de repouso, andar, correr, ataque, habilidade, pesado, super, esquiva, dano, morte e vitória. Cajado, grimório e orbe têm estados próprios.
- Locomoção por distância percorrida, plantio dos pés compartilhado com Brasa e ações no tronco durante o movimento. Sem root motion e sem segundo ator dentro do modelo.
- Instalação nas oito cenas existentes, incluindo CampaignCoop e arenas. O ator de rede clona essa mesma base. Peças antigas ficam referenciadas, mas desligadas.
- Encaixes de arma configurados nos dados e aplicados igualmente no host/réplicas: cajado e orbe com cristal para cima, grimório segurado na mão. Preservado o encaixe anterior dos equipamentos sem configuração própria.
- Oito linhas reutilizáveis para conjuração/carga e selos; trilhas limitadas pelo pool de projéteis, também nas réplicas. Esses efeitos não aplicam dano. Retrato e inventário reutilizam a prévia 3D real do personagem equipado.
- Três sons sintetizados originais para conjuração, impacto e pesado. Corrigida a troca do perfil sonoro ao mudar de herói; anteriormente o feedback podia continuar com o som do Guerreiro.
- Corrigido o relógio enviado para animações remotas de esquiva e reação a dano; antes usava o tempo do ataque mesmo fora de combate.
- Corrigido o indicador de super para usar o ataque/range do super, em vez da habilidade E. O Prisma não recebe o antigo anel laranja da Brasa.

## Identidade e valores iniciais

Vida base 72, velocidade 5,5 m/s; baixa resistência. Equipamento, talentos e modificadores continuam calculados no sistema central existente. Os danos abaixo são bases antes de bônus, crítico e defesa.

| Equipamento | Ataques | Alcance | Identidade |
|---|---|---|---|
| Cajado de Aprendiz | 9 | 8 m | Um alvo, sem explosão ou lentidão gratuita |
| Cajado encontrado | 13 / 13 / 17 | 10 m | Gelo: lentidão de 35% por 1,1s, explosão final maior |
| Grimório encontrado | 13 / 13 / 18 | 9 m | Fogo: explosões de 0,8 / 0,8 / 1,7m |
| Orbe encontrado | 13 / 13 / 8×3 | 8 m | Dispersão final com imobilização de 0,35s; cada projétil é um acerto intencional |

- **Segurar ataque:** sequência automática e pesado após 1,15s de carga; multiplicador 1,8, recuperação de 4s. O pesado preserva o projétil e os atributos da arma; não transforma o cajado inicial em ataque de área.
- **E / habilidade:** aprendiz = Pulso Glacial (14 base, raio 2,2m); cajado encontrado = pulso de gelo; grimório = selo à frente (raio 2,5m, duração máxima 6s, ativa uma vez, imobiliza por 1,2s); orbe = ruptura com afastamento. Habilidades encontradas têm 20 de dano base e cooldown de 8–9s.
- **Q / super cheio:** Convergência, 50 de dano base em 6m e imobilização por 1,5s. Requer 24 acertos confirmados; gasta a barra, tem preparação e não concede invulnerabilidade.
- **Espaço / esquiva:** mantém duração de 0,22s, cooldown de 0,8s e invulnerabilidade só entre 0,03 e 0,15s.

O Foco de Brasa é uma exceção intencional ao cajado regular: preserva projétil mais lento, explosão de 1,15m e sua cadência anterior.

Armaduras, acessórios, artefatos, raridades, talentos, drops e regras de compatibilidade existentes continuam disponíveis. Não há desbloqueio/loja de skins nesta etapa: o modelo próprio é a aparência padrão do Prisma.

## Como conferir

1. Abra o projeto com Unity 6000.3.26f1 ou use o atalho habitual Jogos/Jogar Campanha Forja das Mares.
2. Na seleção de herói, escolha ARCANISTA — Prisma (terceira classe). Entre solo ou em grupo.
3. WASD/setas para andar, mouse para mirar; segure ataque, E para habilidade, Espaço para esquiva, Q com barra cheia. No Android, use os controles equivalentes.
4. Abra Personagem/I: o retrato e a prévia devem mostrar o mago com uma única arma equipada. Cajado, grimório e orbe são equipamentos da classe; obtenha-os jogando. Nenhum item foi adicionado ao save real para teste.
5. Confira os pés ao andar e parar, ações enquanto se movimenta, troca de arma, efeitos, bloqueio de projéteis por paredes e visual visto por outro cliente.

## Fontes e manutenção

- ArtSource/Characters/Prisma: create_prisma.py, create_audio.py, Prisma.blend, preview e relatório geométrico.
- Assets/ForjaDasMares/Art/Characters/Prisma: FBX, prefab, controller, máscara, materiais e áudio.
- Editor/PrismaModelInstaller.cs: instalação idempotente, verifica modelo inesperado/duplicado antes de substituir. Menu Forja das Mares → Characters → Install Prisma model and casting kit. Não é necessário executá-lo para jogar a cena já salva.
- Scripts/Presentation: PrismaCastingView, PooledProjectileTrail; extensões em HeroModelView, HeroPresentation, ActorImpactFeedback, HeroAttackArea, HeroPowerFeedback.
- Scripts/Gameplay: CombatProjectile e CombatEffectPool. Scripts/UI/EquipmentPaperDoll desliga linhas na prévia isolada.
- Networking: CoopActor, CoopReplicaView, CampaignAbilityBar, CampaignHud e diagnóstico de desenvolvimento CoopSmokeProbe.
- Dados: heróis arcanist, armas mágicas/combos/habilidades do catálogo Forge; cenas já existentes. Lista integral no arquivo ARQUIVOS_0_15.txt ao fechar a entrega.

## Validação e limitações

- Instalador importou 25 clipes com curvas e salvou uma apresentação por ator em oito cenas. Modelo também foi inspecionado no Blender em andar, correr, três estilos de arma e super; posições dos ossos sem valores não finitos.
- Primeira rodada focada: 30/32, com duas falhas de contagem causadas por argumento de diagnóstico que sobrepôs o isolamento dos perfis por cena. A reexecução usa o isolamento existente; não houve alteração do inventário para mascarar a falha.
- Suíte geral: 230 casos, 228 aprovados, uma falha e um ignorado. A falha real mostrou que a atualização de categoria reduziu a explosão do Foco de Brasa. Seus dados anteriores foram restaurados e a exceção ficou explícita no instalador.
- Reexecução final após correções e encaixes: **57/57 aprovados**, incluindo os 12 testes do Prisma, armas, campanha, locomoção/Brasa e protocolo. Consolidação do resultado mais recente por caso: 231 aprovados, zero falhas e um ignorado, em 232 casos. Isso não é uma única execução geral final. Ignorado: caso de projeto UGS não vinculado, pois este projeto já está vinculado.
- Duas instâncias Windows reais pelo Unity Relay: ambas Arcanista, mesmo mapa, combos e pesado de cajado/grimório/orbe, clipes e glifos observados no cliente remoto, solicitação de habilidade e encerramento seguro ao sair o host. Sem exceções de referência/índice ou estado de Animator ausente nos logs verificados. Perfis de QA isolados; nenhum equipamento foi injetado no save do usuário. Não foi teste com duas pessoas nem vitória humana completa.
- Prévia do inventário e cena da build conferidas visualmente. A primeira conferência identificou o cajado invertido; corrigido com encaixe configurado por item e aplicado igualmente no host e réplica, depois repetida a sessão.
- Windows e Android finais: Succeeded, errors=0, processos encerrados com código0. APK real 0.15.0/código31, com.forjadasmares.campaign, ARM64, API25–35; tamanho 53.945.755bytes. SHA256 em Evidencias_0_15_0/builds.json. Saídas estáveis: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk. Atualizar Android por cima, sem desinstalar ou apagar dados.
- Houve uma ocorrência de “Crash!!!” no encerramento de um Editor batch após gravar os encaixes. Os 57 testes e a build Windows posteriores terminaram normalmente. Causa nativa não determinada; não confundir com exceção do jogo ou alegar correção do motor.

Celular ausente em adb nesta rodada; não afirmar teste físico desta versão. Sem medições de FPS móvel ou teste humano prolongado de equilíbrio. Modelos e animações são originais e funcionais, ainda sujeitos a refinamento artístico; efeitos de chão continuam sendo indicadores geométricos de protótipo. Não há múltiplas skins desbloqueáveis/loja. Os limites anteriores de host/servidor e progresso por host permanecem.

Evidências: Evidencias_0_15_0/testes.json, windows-relay.json, blender-validation.json, mage-inventory.png e mage-coop.png. Não há novas configurações manuais de pacotes/serviços para jogar esta atualização. Para substituir a arte futuramente, mantenha as referências dos ossos/máscara, os nomes de clipes e o HeroModelView; para ajustar as armas, use customGrip/socketPosition/socketEuler e os ScriptableObjects de ataques.


**Status: aprovado com limitações para testes privados.** Refinamento artístico/equilíbrio humano e teste Android físico continuam pendentes. PC e celular devem usar a mesma versão para ver o mesmo conteúdo.
