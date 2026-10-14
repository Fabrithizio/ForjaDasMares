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
