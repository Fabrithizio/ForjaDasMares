# Paisagens, itens e hordas — revisão0.10.1

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

Protocolo6: usar0.10.1 em todos os participantes;0.9.x não entra. Snapshot de campanha inclui progresso da horda. Até4participantes. Não foram incluídas credenciais ou novos serviços. Salvos versão2/pacoteAndroid/comportamento de conta/host preservados.

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


## Checkpoint e fontes

Checkpoint be974e6 enviado ao GitHub com código, dados e assets. A galeria Blender foi organizada em arquivo separado, com17modelos visíveis e render de revisão. Os ícones são imagens dos modelos de arma, mas os modelos de arma equipados em jogo ainda usam o sistema geométrico anterior; isso está na lista de arte, não foi declarado como arma final modelada.

Correção adicional: EnemyActor/EnemyPresentation restauram os componentes de simulação ao reutilizar um ator que estava como réplica, sem alegar migração de host. O teste correspondente faz réplica→reutilização local autoritativa. O upgrade de combos ignora dados já migrados para não reduzir cadência repetidamente em novas execuções.

## Verificação física e correções adicionais

A build provisória0.10.0 foi gerada para Windows e Android, APK instalado por atualização no moto g15. Teste real pelo Relay no mesmo Wi-Fi com3 processos Windows e1 aparelho Android: quatro vagas, entrada do Android em partida iniciada a0%, mapa e35props iguais, inventário por toque, seleção/equipagem/desequipagem de saque temporário e combate com IA/projéteis. Não são quatro pessoas nem quatro dispositivos independentes.

A equipagem por toque encontrou um defeito real: escala herdada de ossos importados ampliava o cubo da arma até cobrir a câmera. Por isso0.10.0 não foi enviada para compartilhamento. A revisão0.10.1 usa EquipmentVisualPlacement para compensar escala do pai nos sockets e réplicas e reposicionar ao trocar herói. Teste específico cobre host, réplica e troca de visual. Mensagens internas como exploration foram substituídas por texto de recompensa.

Selos foram concluídos por comando administrativo da ferramenta de desenvolvimento, não exploração humana. Os perfis anteriores ao resultado continham exatamente1evento adventure:1000 cada. O Android equipou Machado das Fendas raro ainda na expedição, sem mover o item para o banco; vida máxima140→149, sem curar140HP. Ataque segurado no aparelho matou inimigos reais e carregou SUPER. Três atores morreram pela IA; a derrota final foi provocada administrativamente. Resultado apareceu nos quatro perfis com17XP/9moedas e1/1/1/2itens, expedições concluídas sem duplicar o evento de selos.

Regressão ampla inicial:159casos,156aprovados,2falhas,1ignorado. Uma falha comprovou alteração indevida dos pontos de spawn das Galerias antigas; restaurada a disposição anterior fora da campanha. Outra excedeu180s no teste de recompensa enquanto três clientes também executavam: a causa de timeout ainda exige repetição isolada, não foi considerada aprovada. Evidências estão nos logs/XML locais; registrar o resultado final abaixo após executado.

Repetição focada0.10.1:16/16aprovados,0falhas,32,84s de execução dos casos. Inclui os dois casos antes falhos, horda inteira, escala de equipamento host/réplica/troca de herói, spawn antigo, cura única, modelos em pé, transparência e persistência. XML local wilderness0101-focused.xml. Nenhum erro de compilação nessa execução.

## Builds e teste físico final0.10.1

- Windows: BuildPipeline Succeeded, errors=0, exit0, executável real em Builds/CampaignWindows-0.10.1/ForjaDasMaresCampanha.exe. Inspeção visual no PC confirmou inventário com ícones/comparação, item equipado na prévia em escala normal, início solo e cenário ampliado sem arma cobrindo a câmera.
- Android: exportação/compilação C#/IL2CPP concluídas; empacotador interno falhou com java.io.IOException Unable to establish loopback connection. O mesmo projeto Gradle exportado foi compilado externamente com clean assembleDebug: BUILD SUCCESSFUL em2m40s,69tarefas, exit0. Não foi apresentada a execução interna falha como build aprovada.
- APK real: Builds/CampaignAndroid-0.10.1/ForjaDasMaresCampanha.apk,52.016.179bytes, versão0.10.1/code24, com.forjadasmares.campaign, ARM64, mínimoAPI25/alvo35, assinaturaAPK v2 verificada. SHA256 A97BE8B75AB8F27DC212BA3709A02A23413EA267C0646C74D5C00442987E5444.
- adb install -r retornouSuccess no moto g15. Tela inicial mostrou0.10.1. Entrou por código pelo Relay em hostWindows0.10.1; duas vagas reais PC+Android no mesmo Wi-Fi. Inventário preservado: dois machados do resultado anterior. Equipagem por toque validada no host: máximo140→149, Machado das Fendas raro na prévia, arma proporcional no mapa. Joystick deslocou o ator e câmera acompanhou; ataque segurado enviado por toque. Interações verificadas com arquivo de evidência visual local wilderness0101-phone-*.png.
- Comando administrativo encounter apenas posicionou a dupla num encontro real; nenhuma ordem defeat foi usada nessa rodada final. IA levou ambos a0HP, estadoDefeat sincronizado e enemies/effects0. Não é teste de habilidade humana nem vitória completa. PC e Android transitaram Connecting→Lobby→Loading→Playing→Defeat e depoisClosed quando host saiu. Logs específicos do teste sem NullReferenceException/MissingReferenceException/IndexOutOfRangeException/InvalidOperationException/fila cheia. Salas de QA encerradas.
- Arquivo compartilhável copiado para Desktop/Jogos/Enviar para meu irmao/ForjaDasMares-0.10.1-Android.apk, com COMO JOGAR.txt atualizado. Ambos devem atualizar. Não foi enviado WhatsApp automaticamente nem publicado na Play Store.

Pendências priorizadas: importantes — medir CPU/GPU/memória/FPS e rede sob horda em aparelhos reais; balancear as três fases em sessões humanas; revisar mapas ainda em grade e grupo espalhado com encontro serializado; finalizar modelos de arma, rigs/clips dos demais heróis/inimigos. Melhorias opcionais — ícones exclusivos por item, som/música final, tradução dos estados de sessão que ainda aparecem em inglês no mapa de resultado. Sem quatro dispositivos ou redes diferentes, migração de host, servidor dedicado ou aprovação comercial.

## Resultado final

Regressão PlayMode0.10.1:160casos,159aprovados,0falhas,1ignorado; execução595,18s, XML wilderness0101-full.xml. O ignorado exige projetoUGSdesvinculado, incompatível com o vínculo necessário ao teste real atual; não foi contado como aprovado. Não há bloqueador comprovado restante nos fluxos cobertos nesta entrega. As duas falhas da primeira rodada foram resolvidas/repetidas com sucesso. Arquivo de ruído de importaçãoURP teve duas referências de configuração restauradas ao estado versionado após todos os processosUnity fecharem; nenhuma configuração de jogabilidade foi alterada depois das builds.

Status: aprovado com limitações para testes privados0.10.1, com evidências acima. Os limites de arte, encontro serializado, balanceamento/desempenho e redes distintas permanecem. Próximo trabalho recomendado: sessão humana nas três fases para priorizar problemas observados. Relação completa: ARQUIVOS_0_10.txt. Builds e evidências locais não são enviadas ao Git; código/dados/fontes/documentos são o backup.
