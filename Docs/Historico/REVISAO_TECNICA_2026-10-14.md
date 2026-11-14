# Revisão técnica — 14/10/2026

## Estado por sistema

| Sistema | Estado/evidência |
|---|---|
| Movimento, colisões, câmera, esquiva | Implementados; testes da base repetidos e aprovados; entradas por adaptador/Input System. |
| Quatro heróis, habilidades, combate | Implementados com geometria/poses provisórias; ataques reais e cooldown/invulnerabilidade finitos. Sem clips/modelos finais. |
| Cinco inimigos, invocações e chefe | Implementados; IA, navegação, ondas e pools; testes dos quatro heróis contra arquétipos/chefe. |
| Três biomas e masmorras | Implementados;12.000 combinações lógicas e12 gerações físicas testadas; portas/objetivos/saída integrados. |
| Progressão local | Integrada nas variantes ProgressionDungeon/ProgressionArena; catálogo, equipamento, inventário, resultado, mascote e save.68 testes PlayMode aprovados no conjunto. Builds/executável seguem em verificação separada. |
| Interface | HUD, controles iniciais, inventário/comparação/descarte, entregas, resultado e opções simples funcionais. Menu principal/lobby/pausa completa/ajustes de toque aguardam a etapa de interface. |
| Rede | Não implementada nesta revisão. Nenhum pacote NGO/Transport/MPS foi instalado neste checkpoint. Sem crossplay testado. |
| Conteúdo adicional/lançamento | Ainda não implementados/validados. Progresso de código não equivale a pronto para lançamento. |

## Inspeção e problemas corrigidos

- Compilação Unity disponível e executada;68/68 PlayMode,0 falhas. Os45 testes antigos foram conservados, acrescentando domínio de save/inventário, integração e percurso completo com equipamento/loot.
- Auditoria de4 cenas,7 prefabs,92 definições e416 MonoBehaviours:0 scripts ausentes,0 referências obrigatórias/quebradas,0 problemas reportados. Varredura inclui objetos inativos e referências serializadas. Não garante ausência de todos os defeitos de runtime.
- Sem nomes de scripts próprios duplicados, métodos vazios ou TODO/FIXME críticos encontrados. Sem UnityEditor em runtime. Os3 asmdefs próprios não formam ciclo. O validador de dados deixou de depender do cálculo de gameplay para limites de afixo.
- MeleeAttack e CombatFeedback são legado referenciado e desativado nas cenas de heróis. Não apagados. A geometria/cenas originais foi preservada; ArenaTest SHA25607D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4; DungeonTest SHA2563FD8D2FDF6657A8DF99553CD991F320D770F7FDDDA468649B14C1A88BAB1503F.
- Corrigidos nulls materializados por JsonUtility, comparação de duas mãos, valores NaN/enums inválidos, perda de XP quando loot opcional está cheio, limpeza após inicialização incompleta, presença/checksum no envelope de save, preservação do último resultado no abandono vazio e arma equipada acompanhando a apresentação.
- Ledger deduplica concessão e conclusão; múltiplos colliders não duplicam golpe. Segundo golpe de adaga, cada projétil de leque e pulso lendário são acertos adicionais intencionais/testados.
- Eventos de bridge, HUD e equipamento são removidos ao destruir componentes. Referências a atores/zonas/projéteis são liberadas nos pools; scene bridge não atualiza UI durante destruição. Singleton de perfil tem reset de subsistema e limpeza; testes de transição verificam uma instância.

## Limitações e próximos bloqueadores

1. **Bloqueador para online:** implementar rede e configurar projeto/serviços; perfil local é editável e não pode ser autoridade econômica. Auditar/migrar com host validando intenções, RNG, dano, loot e conclusão.
2. **Bloqueador para lançamento:** sem sessões reais Windows/Android/iOS, profiling físico, arte/animações finais, fluxo completo de menus ou testes prolongados. Não há aprovação de lançamento.
3. **Importante:** salvar por concessão garante consistência mas deve ser medido em celulares; transferência interrompida usa metade de XP/moedas e conserva itens. Não retoma combate de uma masmorra interrompida.
4. **Importante:** controles físicos/gamepad, safe areas e ergonomia móvel exigem aparelho. As opções atuais são volume global/texto/mascote; não equivalem à camada completa de acessibilidade.
5. **Melhoria:** balancear com sessões humanas, fazer profiling do custo de navmesh/consultas/UI/gravação, substituir placeholders por assets licenciados sem redistribuir arquivos restritos em Git público.

Status da base local e progressão: **aprovado com limitações**, condicionado à inspeção final do executável/builds registrada em PROGRESSO.md. Status online/lançamento: **não aprovado ainda**, porque os recursos/testes não existem. A autorização posterior do usuário permite continuar pela ordem, sem confirmação presencial; próximo passo só após a entrega local validada.
