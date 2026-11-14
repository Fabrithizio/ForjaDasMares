# Inventário visual 0.15.6

## Sete melhorias
1. Retratos de item 92px na inspeção, nome e raridade destacados; metadados separados.
2. Tabela com oito atributos: dano, vida, defesa, chance/dano crítico, cadência, movimento e redução de recarga. Colunas atual/ao equipar/diferença, setas e cores; diferenças percentuais em pontos percentuais.
3. Cartões distinguem ATAQUE e BÔNUS/EFEITO. Dados vêm do catálogo e da instância reais.
4. Resumo sem rolagem: comparação essencial sempre visível. Descrição técnica, modificadores completos e demais atributos disponíveis em Avançado com rolagem.
5. Ação principal destacada; desequipar somente para peça equipada neste herói; equipar/reciclar para peças soltas; entregas mantêm receber sem equipar automaticamente.
6. Ícones da mochila ampliados: 66px PC / 80px toque. Bordas por raridade, destaque da seleção, peças incompatíveis dessaturadas com motivo.
7. Celular e janelas inicialmente menores que 1000px usam quatro itens por página em cartões maiores (2x2), contra oito no PC (4x2). Inspeção móvel tem abas VER EQUIPADO / VER SELECIONADO e tabela sempre presente. As abas consultam os dois cartões; nunca trocam o item usado como referência.

## Arquitetura e arquivos
- Novo Assets/ForjaDasMares/Networking/InventoryComparisonView.cs (+meta): apresentação somente, sem pedidos de equipamento/recompensa e sem serialização de inventário. Calcula a prévia com ProgressionStats.Calculate/CanEquip.
- CoopInventoryHud.cs: integra a apresentação, paginação por layout, destaques e ações por estado; mantém autoridades e intenções existentes.
- Networking/Editor/CampaignInstaller.cs e ProjectSettings/ProjectSettings.asset: versão 0.15.6 / Android código37.
- Docs/PROGRESSO.md: registro da entrega.

## Preservado
Comparação automática com equipado do mesmo slot; vazio sem referência da mochila; regras de classe, duas mãos e secundária; confirmação de reciclagem; perfil local/remoto, saque e entregas; saves v2/protocolo10. Valores das referências visuais não foram copiados para dados de combate. Nenhum slot fictício adicionado.

## Como conferir
Abra Personagem e Equipamentos. Selecione duas armas e confira que a referência continua sendo a equipada. Veja antes/depois na tabela; compare padrões de ataque e efeitos. Abra item equipado para conferir ação DESEQUIPAR. Abra Avançado e volte ao resumo. Troque classe/filtro, pagine e confira incompatibilidade. No Android confira 4 itens por página e as duas abas; a tabela permanece com a mesma prévia ao alternar abas.

## Verificação e limites
PC: build Succeeded/errors0/exit0 (Logs/inventory-0156-pc.log), conferência manual de mochila, arma solta e equipamento atual: oito atributos legíveis, blocos separados, ações condicionais. Nenhuma das exceções NullReferenceException/MissingReferenceException/ArgumentException/Cannot add encontrada no Player.log nesta conferência. Novos testes automatizados não foram executados.
Android: primeira build Succeeded/errors0/exit0; instalada com install -r Success, versão0.15.6/código37 confirmada. Conferência real no moto g15 (2400x1080): mochila 2x2 com quatro itens, filtro todas as classes e navegação página1/3→2/3; itens de outra classe dessaturados com aviso; armadura sem equipada mostrou prévia de vida103→123 e defesa5%→17%; alternar VER EQUIPADO/VER SELECIONADO manteve esses valores; Orbe do Arcanista inspecionado com Guerreiro teve EQUIPAR desabilitado e motivo explícito. Itens e ouro não foram alterados. Após essa conferência, corrigidos textos de slot sem arma/armadura e unidade golpes/s. Builds finais Windows e Android Succeeded/errors0/exit0 (Logs/inventory-0156-pc-final.log e inventory-0156-android-final.log). Unidade golpes/s conferida na tabela do executável final de PC. APK final atualizado via install -r Success; versão0.15.6/código37 confirmada. Conferida novamente no aparelho a armadura selecionada: texto corrigido, cadência em golpes/s e tabela com vida103→123/defesa5%→17%, sem cortes na área essencial. Arte/ícones existentes reutilizados; esta etapa não cria uma coleção nova de assets finais. O layout é escolhido ao construir a interface; redimensionar uma janela através do limite de 1000px exige reiniciar para mudar de modo. Textos extensos têm versão completa em Avançado; os cartões essenciais mostram até dois bônus passivos e o efeito principal. A tabela essencial contém oito atributos; os demais continuam em Avançado. Sem novo teste de sessão online nesta etapa de interface.

## Dependências/configuração
Nenhum pacote novo. Usa uGUI, dados e ícones existentes. CoopInventoryHud constrói a nova apresentação automaticamente na campanha; não é necessário adicionar um componente manual às cenas ou mudar saves.
