# Desequipar com mochila cheia — 0.15.7

## Causa
ProfileService.Unequip recusava a operação quando a retirada de uma ou duas peças excedia a capacidade da mochila. Isso acontecia mesmo em um slot válido e fora de combate.

## Correção
- Espaço disponível: peça retorna à mochila; item temporário permanece no saque da expedição.
- Mochila cheia: instância permanente excedente é movida para Entregas e o slot fica vazio.
- Retirar principal também remove a secundária quando necessário às regras; cada instância é tratada uma vez.
- Transação continua validada e salva antes de substituir o perfil ativo; falha de salvamento não destrói o estado anterior.
- Sem criação de cópias, moeda ou alteração de atributos/raridade do item.
- Todas as categorias usam o mesmo serviço. As restrições de segurança da sessão/combate e as regras de compatibilidade não foram removidas.

## Arquivos
- Scripts/Gameplay/ProfileService.cs: remoção do bloqueio de capacidade e destino seguro.
- Networking/Tests/RpgTests.cs: expectativa do teste existente FullBagPreservesUnequippedItemInDeliveries atualizada para a regra nova; não executado neste bloco.
- Networking/Editor/CampaignInstaller.cs / ProjectSettings/ProjectSettings.asset: 0.15.7, Android código38.
- Docs/PROGRESSO.md: registro da entrega.

## Verificação
Windows build Succeeded/errors0/exit0. Conferência no executável real, perfil do jogador: mochila36 e entregas1 antes; Corta-Maré incomum equipada no Guerreiro. Clique em DESEQUIPAR esvaziou o slot e mostrou a mesma peça em Entregas (agora2), mantendo mochila36 e ouro1309. Comparação dos registros de item antes/depois no salvamento confirmou todos os IDs/atributos preservados e sem IDs duplicados; a arma retirada apareceu exatamente uma vez em Entregas. Evidência local Logs/unequip-0157-verification.json (sem publicar o perfil do jogador). Jogo encerrado e reaberto: slot principal continuou vazio, mochila36/Entregas2 e ouro1309 preservados, confirmando o carregamento do salvamento.
Android build Succeeded/errors0/exit0; APK0.15.7/código38 no caminho estável, instalado via install -r Success no moto g15, versão0.15.7/código38 confirmada; dados preservados. Interação física desta correção ainda não conferida no Android. Não executada suíte automatizada nem sessão online nova neste bloco. Protocolo10 e savev2 preservados.

## Como conferir
Abra Personagem, clique no slot ocupado e em DESEQUIPAR. Se a mochila estiver cheia, abra ENTREGAS para localizar a peça. Receber de Entregas ainda exige espaço livre: recicle uma peça que não usa ou equipe uma peça solta da mochila antes de receber. Itens do saque atual continuam na aba SAQUE até o fim da expedição; não são convertidos em recompensa permanente só por desequipar.
