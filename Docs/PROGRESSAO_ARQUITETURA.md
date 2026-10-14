# Progressão local — etapa 6

Auditoria: Unity 6000.3.26f1, URP17.3, Input System1.14.2, AI Navigation2.0.16; checkpoint inicial 3a4d1e4. Não há inventário/persistência equivalente. Reutilizar Health, HeroRoster, HeroCombat, DamageResolver, CombatEffectPool, EnemyPool, EncounterController e DungeonSession. Preservar ArenaTest e DungeonTest; criar variantes ProgressionArena e ProgressionDungeon copiando as cenas já integradas, acrescentando composição de progressão.

## Decisões

- Catálogo ScriptableObject: equipamentos, regras por herói, raridades, recompensas e mascote. Instâncias guardam ID único, definição, raridade e modificadores limitados; os assets não são estado de jogador.
- Fórmulas centralizadas em ProgressionStats. Nível compartilhado pelo perfil; conjuntos de equipamento separados por herói. Equipar não cura nem revive. Equipamento só pode mudar fora de encontros e de ataques/projéteis ativos. Armas alteram alcance, arco, cadência, combo, número/velocidade de projéteis, impacto e controle.
- Serviço local de perfil separado do componente de cena. Toda alteração permanente é uma transação gravada antes de publicar o novo estado. Inventário configurável e caixa de entregas para recompensas quando cheio, sem descarte automático em transições.
- Expedição com ID próprio e ledger de eventos. Morte ou abandono mantém os itens obtidos e concede metade de XP/moedas; vitória concede tudo. Reinício/aplicação interrompida encerra a expedição pendente como abandono antes de iniciar outra. Não retomar geometria/combate salvo nesta etapa. Relatório permanece no perfil.
- Save versionado, validação e migração explícita, checksum para detectar corrupção, escrita temporária com flush e substituição atômica com backup. Sem segredos. Checksum e arquivos locais não oferecem autoridade para uma economia online: backend futuro deve recalcular e validar concessões.
- Mascote de suporte geométrico sem collider, cura limitada por cooldown e orçamento de cargas da sessão; segue/teleporta perto do herói, nunca bloqueia passagens.

## Fórmulas previstas

Nível máximo20; XP para próximo nível = 60 + 35×(nível−1). Vida base cresce3% por nível; dano2,5%. Bônus de itens são somados uma vez por slot e limitados. Defesa adicional usa defesa/(defesa+100), redução total limitada55%; velocidade extra≤25%, crítico≤30% (multiplicador1,5), velocidade de ataque≤45%, redução de cooldown≤35%, bônus elemental≤25%, recuperação≤2HP/s. Regras e probabilidades ficam no catálogo. Sem multiplicadores recursivos ou afixos infinitos.

## Verificação a executar

Catálogo/compatibilidade/raridades; limites e confirmação de descarte; equipar/desequipar com comparação; alcance/padrão/dano/cadência reais; slots secundários e escudo frontal; recompensa repetida rejeitada; baú/chefe/morte/saída/transição; save/reload, migração, corrupção/backup, falha de escrita; mascote sem colisão/cura infinita. Reexecutar os45 testes existentes. Build Windows com -buildTarget Win64 e inspeção do executável; preparar Android sem alegar teste físico ausente.

## Integração implementada

Catálogo real em Data/Progression/Catalog.asset;12 equipamentos e seus ataques,9 categorias,5 raridades e9 afixos possíveis. ProgressionInstaller cria apenas variantes novas das cenas existentes e configura referências, input de menus, arma geométrica e mascote. ProgressionHud usa os serviços reais para equipar, receber entregas, descartar confirmado e salvar opções. Cliques em UI são filtrados no input de ataque das variantes; movimentos/habilidades ficam bloqueados enquanto o painel está aberto, sem alterar Time.timeScale.

Health.ApplyAttributes conserva vida atual, inclusive zero; PlayerMotor recebe velocidade e cooldown recalculados a partir da base; HeroCombat recebe padrões e atributos. O multiplicador de dano/crítico/elemental é calculado ao iniciar cada ação e copiado aos projéteis/zonas, impedindo mudanças no meio de um voo. Crítico é uma decisão por ação, máximo30%, multiplicador1,5. Duas adagas aplicam dois golpes intencionais com ledger separado; o leque possui três projéteis de dano menor. FrontalBlock é aplicado por HeroEquipment através de IHitDefense antes da defesa.

Eventos opcionais de DungeonSession e EncounterController alimentam ProgressionSceneBridge somente nas variantes. EnemyPool emite morte uma vez por ativação; o bridge associa uma sequência de spawn por expedição. Minions não rendem loot. Baú só muda visual/estado depois de salvar sua concessão; saída só termina depois do commit. Morte/abandono/transição usam o mesmo caminho idempotente. OnApplicationQuit também encerra a concessão pendente; nova abertura trata interrupção anterior. O relógio/autoridade é local nesta fase.

Save: payload versão2 e envelope versão2 com flags explícitas de presença, pois JsonUtility pode materializar null como objeto vazio. Checksum cobre flags e payload. Envelope1 tem migração de sentinelas vazias; payload1 recebe campos adicionais. Versões futuras/corrupção são rejeitadas. Gravação é temporária, Flush(true), File.Replace com backup; erro retorna sem publicar o estado candidato. Não há autenticação do arquivo, criptografia secreta ou proteção contra adulteração intencional. Migração de contas/economia para rede exige uma autoridade externa.

Nível é compartilhado; cada herói possui loadout próprio. Vida adicional total≤160; bônus de dano de equipamento≤50%; defesa combinada≤55%; bônus de movimento≤25%; ataque≤45%; cooldown reduzido≤35%; elemental≤25%; recuperação≤2HP/s; escudo≤50% frontal. Elemental é dano extra do golpe, sem sistema separado de resistências nesta etapa. Recuperação não ultrapassa vida máxima nem revive. Bônus não se multiplicam por reequipar. Capacidade temporária32 com3 espaços reservados para chefe/saída; inventário24 e entregas até o limite de segurança512 itens combinados. Ao atingir o limite, uma nova expedição é recusada até liberar estoque. Saque opcional cheio não impede XP de inimigo.

Validação intermediária:67/67 PlayMode aprovados, incluindo percurso completo com equipamentos e resultado salvo. Auditoria Editor:4 cenas,7 prefabs,92 definições,416 behaviours,0 scripts ausentes,0 referências ausentes. Verificação final e builds continuam separadas; consultar PROGRESSO.md para estado efetivo.
