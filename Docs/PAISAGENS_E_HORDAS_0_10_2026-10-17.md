# Paisagens, itens e hordas — revisão0.10.0

## Pedido e auditoria

O usuário pediu mapas maiores/abertos, inventário com imagens e uso de saque durante a fase, mais inimigos e elites, combos, habilidades, objetos de cura e assets originais. Auditados CampaignRun, DungeonSession/Generator/Layout, EncounterController/EnemyPool, HeroCombat, ProfileService/Store, CoopProgression/ProfileChannel/ReplicaView e HUD existentes. A campanha usava setores de12m em grade16m, portas durante encontros, dez inimigos ativos e saque equipável só depois do resultado. Arquitetura e perfis existentes preservados.

## Implementação

- Campanha usa openLandscape por nível, setores28/30/32m, centros separados por tamanho+4m e passagens18m. As três rotas existentes de12/15/18setores foram preservadas; área de cada setor passou de144m² para784/900/1024m². As cenas de arena e masmorras anteriores continuam com tamanho e portas originais.
- Portões da campanha permanecem abertos. Transitar não cancela o encontro. O controlador conserva o setor dono do combate e credita a conclusão a ele, mesmo com jogadores em outros lugares. Há um encontro ativo de cada vez; setores adiante aguardam a conclusão do anterior para iniciar seu combate. Não foi criado um sistema de várias simulações independentes simultâneas.
- A saída final exige concluir os combates e objetivos obrigatórios, para não permitir correr direto ao chefe e concluir sem lutar. Baús e descanso continuam opcionais. Placas e armadilhas funcionam nos setores visitados mesmo quando outro jogador muda o setor atual.
- Objetos extras de cobertura e decoração por bioma. Suprimentos quebráveis curam22HP de um aliado vivo ferido a até8m, respeitando linha de visão, uma vez; não ressuscitam, não curam todo o grupo infinitamente e não dependem de repetir concessão de moeda.
- Encontros difíceis têm mais de100 inimigos acumulados em4–6ondas. Limites20simultâneos no mobile/24noPC. Raider tem28HP e golpe6; pressão vem da composição/quantidade, não de inflar vida indiscriminadamente. Percepção/alcance, bloqueio e ataques telegrafados continuam nos sistemas existentes.
- Três elites: Malho do Breu, Oráculo das Raízes e Rasga-Cinza. Respectivamente varredura pesada, marcas de área e investida; cada um também tem retaliação e fase por vida. Vida320/220/260, preparação principal1,15s, recompensa configurável própria. Não são três chefes finais novos com clips artísticos.
- Pool de tipos cheios espera por vagas daquele tipo, evitando falha espúria quando há vagas apenas em outro tipo.
- Hordas mostram onda e quantidade de derrotados/planejados. Autoridade de dano/IA/RNG/saque segue no host.
-12ícones de categorias renderizados de meshes originais no Blender; dados EquipmentDefinition.icon configurados. Cartões com imagem, raridade em português, detalhes roláveis, slot, exigência de nível, comparação de vida/dano/defesa/velocidade/cadência e instrução de uso.
- SAQUE ATUAL mostra os itens temporários da expedição. Pode equipar fora de perigo: sem inimigo a12m na campanha aberta, sem ação/efeito ativo, validado pelo host. Esses itens permanecem separados até o resultado. Perfil.Item procura inventário e expedição; terminar não duplica instâncias. Se a mochila estiver cheia, equipamento usado é preservado deslocando item guardado não equipado para entregas; se não houver espaço seguro, o item vai a entregas com referência desequipada.
- Armas principais têm sequência de três ataques configurada: cortes/estocada/finalizador ou tiros/rajada/área conforme categoria. Cadência e alcance variam. Ataques reais permanecem em HeroCombat, não em animação sem dano.
- Patrulheiro: carregado e SUPER de leque de projéteis; Arcanista: nova com controle; Cavaleiro Rúnico: impacto e proteção temporária dos aliados. Guerreiro mantém seu fluxo existente. Cliente envia intenção, host calcula carga/consumo. SUPER24acertos nas três classes adicionais; proteção40%por4s, sem acumulação ilimitada. Outros heróis ainda não receberam modelos/clips finais.
-17modelosFBX,12íconesPNG e4sonsWAV originais. Fonte.blend e geradores em ArtSource/ExpeditionSet. EnemyModelMotion movimenta membros conforme deslocamento e estado, incluindo réplicas; não é um conjunto de clips finalizados. Sons conectados a deltas reais de vida/saque, com limite de frequência e volume master existente.
- CameraOcclusion foi adaptado a colliders com renderers filhos, para que os modelos importados também fiquem parcialmente transparentes ao ocultar o jogador. Grupos de renderers são armazenados por collider e limpos no reset; sem GetComponentsInChildren por frame.

## Rede e salvamento

Protocolo6: usar0.10.0 em todos os participantes;0.9.x não entra. Snapshot de campanha inclui progresso da horda. Até4participantes. Não foram incluídas credenciais ou novos serviços. Salvos versão2/pacoteAndroid/comportamento de conta/host preservados.

Eventos por expedição limitados a2048 para suportar fases maiores; deduplicação permanece. Espelho enviado ao cliente omite o histórico interno de eventos, mas preserva itens/contadores, evitando aumentar a mensagem com centenas de IDs. O cliente continua sem permissão de conceder recompensas. Saque de inimigos reduzido para2,5%para compensar quantidade; XP/moeda básicos1por inimigo. Elites têm tabela própria de1item incomum+,12XP/8moedas, sujeita ao limite de saque. Reservas para chefe/final/selos permanecem.

## Verificação em andamento

Upgrade inicial executou sem erro de compilação e salvou a cena existente. Primeira rodada rede/campanha:46total,44passaram,1falha por teste esperar protocolo5,1ignorado por exigir UGSdesvinculado. Expectativa corrigida para6. Testes focados WildernessTests:5/5passaram, incluindo horda inteira com spawn real do controlador e mortes administrativas nos testes, transição sem apagar luta, dados/assets, cura única e persistência de item equipado com inventário cheio. Isso não é uma vitória humana nem medição deFPS.

Registrar abaixo builds, regressão completa e teste físico após executar. O documento de assets é ASSETS_E_DIRECAO_0_10.md.

## Limitações e próximos testes

- Mais de100 é contagem acumulada; não foram prometidos100agentes simultâneos. Performance precisa de medição no moto g15 com20ativos, projéteis e efeitos, e em outros aparelhos.
- Mapas continuam modulares numa grade com objetivos internos, embora maiores e com passagens abertas. Não são terreno orgânico artesanal ou mundo aberto contínuo ilimitado.
- Combates ao longo do percurso são serializados pelo controlador atual. Grupo muito espalhado pode precisar voltar ao setor de combate ativo.
- Ainda é necessário ajustar dificuldade e duração em sessões humanas completas; não há aprovação comercial ou garantia de cinco minutos exatos.
- Ícones são por categoria, não exclusivos de cada item; materiais planos e animação procedural de inimigos ainda precisam de arte final.
- Sem teste de redes/operadoras diferentes, migração de host, servidor dedicado, contas comerciais ou publicaçãoPlayStore nesta revisão.
