# Inventário 0.15.3 — clareza dos itens

## Implementado
- Mochila, saque da expedição e entregas em abas separadas, com contagens e explicação de origem.
- Filtros por classe e slot, paginação, ordenação visual por slot/raridade/nome e orientação para filtros sem resultados.
- Itens equipados ficam representados nos slots do herói e não na grade da mochila. Ícones e silhuetas existentes preservados; rótulos identificam a função do slot.
- Resumo do item informa classe, nível, onde equipa, restrição de duas mãos, dano numérico da arma, alcance, padrão real do ataque, bônus e ouro por reciclagem.
- Comparação da ficha usa ProgressionStats.Calculate; comparação entre bônus de itens aparece em seção separada. Detalhes avançados opcionais, com rolagem.
- Motivos de bloqueio de equipagem visíveis. Receber entrega aguarda confirmação de propriedade e abre o item; não equipa automaticamente. Reciclagem exige dois cliques, com valor de ouro explícito.
- Perfil deste aparelho e cópia da última sala identificados. Organização na cópia avisa que será validada ao retornar à origem.

## Como conferir
1. Abra o jogo pelo atalho habitual, escolha Arcanista e abra Personagem.
2. Use os filtros de classe e categoria; clique em um slot vazio para filtrar sua categoria.
3. Clique em um item: confira ONDE EQUIPA, CLASSE, nível, ataque e bônus.
4. Compare FICHA AO EQUIPAR; use VER DETALHES AVANÇADOS e OUTRO ITEM DE REFERÊNCIA.
5. Equipe um item compatível: sai da grade e ocupa o slot. Desequipe para devolvê-lo à grade.
6. Na partida, SAQUE mostra os itens temporários; ENTREGAS permite receber itens pendentes se houver espaço.
7. RECICLAR mostra ouro e pede confirmação no segundo clique. Não teste com um item valioso.

## Verificações
- 56/56 testes PlayMode finais, Unity exit0: InventoryClarityTests (12), PrismaTests (16), AimTargetSelectorTests (3), CoopProgressionTests (8), CoopProtocolTests (3), ProgressionAuditTests (14).
- Casos incluem equipar/desequipar, persistência após recarregar o serviço, entrega sem equipagem automática, confirmação e ouro único, comparação pela fórmula real, referência secundária válida, textos longos e navegação sem mutação do perfil.
- Conferência visual Windows 1280×720 em perfil isolado: grade, resumo, comparação e equipagem real do grimório no Arcanista. Perfil normal do jogador não utilizado nesses testes.
- Evidência: Evidencias_0_15_3/playmode-56.xml e captura pc-item-resumo.png.
- Builds Windows e Android Succeeded/errors0/exit0. APK 0.15.3/código34/ARM64/API25–35; instalação por cima no moto g15: Success, versão conferida.
- Conferência física Android 2400×1080: mochila, resumo, comparação e equipagem por toque em perfil isolado. Aplicativo reaberto em modo normal ao final. Capturas android-*.png e hashes em entrega.json. Não foi executada nova sessão de rede entre plataformas neste bloco.

## Arquivos de código
- Scripts/UI/ItemDescriptions.cs: resumo, padrões reais e comparação centralizada.
- Scripts/Gameplay/InventoryRules.cs: consulta de item equipado pelo herói.
- Networking/CoopInventoryHud.cs: abas, filtros, cartões, detalhes, avisos e recebimento.
- Networking/CampaignProfileHub.cs: origem do perfil e aviso de validação de cópia.
- Networking/Tests/InventoryClarityTests.cs (+meta): 12 testes integrados.
- Networking/Tests/ForjaDasMares.Networking.Tests.asmdef: referência Unity.ugui para testar botões reais.
- Networking/Editor/CampaignInstaller.cs e ProjectSettings/ProjectSettings.asset: versão 0.15.3, código Android 34.
Caminhos relativos a Assets/ForjaDasMares, exceto ProjectSettings.

## Dependências e limites
Sem novos pacotes, sem alterações de fórmulas, economia, save v2 ou protocolo10. Mesma aplicação Android com.forjadasmares.campaign; atualizar por cima. Artes/ícones existentes reutilizados, sem alegar assets novos. A descrição mostra dano da arma sem bônus do herói; a comparação mostra dano final antes da defesa inimiga.

A cópia da última sala não é uma conta global: autoridade/economia continuam no host. Um cliente pode habilitar botão antes de saber de um efeito próprio ativo; host revalida e informa recusa. Nova sessão PC–Android, longas campanhas, quatro jogadores e todas as combinações de equipamento não foram revalidados neste bloco. Fluxo geral de menus permanece para outro bloco; nenhuma reformulação de combate foi adicionada.

Limitação visual observada: grimórios grandes podem ultrapassar o recorte da prévia do personagem. Os ícones dos slots e as descrições continuam visíveis; ajuste de enquadramento da prévia fica pendente.
