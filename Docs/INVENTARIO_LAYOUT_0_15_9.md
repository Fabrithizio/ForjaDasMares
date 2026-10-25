# Inventário de PC — 0.15.9

## Alterações
- Apresentação em três colunas, guiada pela referência: mochila, herói/equipamentos e item selecionado.
- Identidade central com classe selecionável, nome do herói, medalhão de nível, carteira de ouro e barra de XP com progresso atual/necessário. Dados do perfil ativo, inclusive o espelho da última sala quando selecionado.
- Grade de 16 itens por página no PC, bordas de raridade/seleção, categorias, filtro de classe e capacidade compartilhada visível.
- Personagem 3D real reutilizado, cinco slots reais com ícones; atributos compactos e acesso à tela de talentos existente.
- Seleção abre detalhes na coluna direita, sem cobrir a mochila. Comparação automática com o equipado no mesmo slot fica fixa; descrição/ataque/bônus/efeitos têm rolagem. Botão de comparação completa preserva a inspeção detalhada anterior.
- Equipar, desequipar, reciclar com confirmação, receber entrega, receber todos, fonte de perfil, troca de herói e paginação usam as funções existentes. Nenhuma alteração de fórmulas, itens, inventário persistente ou autoridade.
- Arte de fundo, ícones e modelo existentes reutilizados. Não representa reprodução exata das ilustrações/pintura da referência, nem novos modelos finais.

## Arquivos
- `CoopInventoryHud.cs`: grade e ligação da apresentação com seleção/ações existentes.
- `CoopInventoryHud.Desktop.cs` e `.meta`: layout específico de PC, identidade e detalhes laterais.
- `CampaignCharacterSheet.cs`: redistribuição das cartas de atributos e botão de talentos no inventário de PC.
- `CampaignInstaller.cs` e ProjectSettings: versão 0.15.9; futuro código Android 40, sem gerar APK.
- `PROGRESSO.md` e este documento.

## Conferência
- Unity 6000.3.26f1: build Windows Succeeded, errors=0, saída 0 após corrigir uma referência de classe durante a compilação inicial.
- Compilação final após os ajustes de visibilidade da tela de resultado: Succeeded, errors=0, saída 0; log `Logs/inventory-0159-pc-final.log`.
- Aplicação Windows aberta, início exibiu 0.15.9. Revisão por cliques interrompida por interação do usuário com a janela; não declarar a conferência visual completa nem fluxo funcional validado desta versão.
- Não foram executados novos testes automatizados, sessão de rede ou teste físico de celular. Save v2 e protocolo 11 preservados.
- Android mantém o layout de toque compartilhado anterior; não foi gerado ou instalado APK, conforme pedido.

## Como conferir no PC
1. Abra `Builds/Atual/Windows/ForjaDasMaresCampanha.exe`, depois PERSONAGEM E EQUIPAMENTOS.
2. Veja nome, nível, ouro e XP no centro; compare com a tela de talentos/perfil ativo.
3. Selecione armas, armaduras, acessórios e artefatos. À direita deve aparecer somente a referência equipada no mesmo slot, ou SLOT VAZIO.
4. Role a descrição; a comparação fixa e os botões devem continuar acessíveis. Abra DETALHES E COMPARAÇÃO COMPLETA e volte.
5. Equipe/desequipe, filtre e troque página/herói. Verifique os cinco ícones, quantidade da mochila, ENTREGAS e RECEBER TODOS quando houver excedente.
6. Abra ATRIBUTOS E TALENTOS e volte. A tela de resultado continua disponível ao concluir uma partida.

## Limitações
- Revisão visual e cliques do novo layout pendentes se a janela continuar indisponível para controle.
- Arte dos itens/modelos segue a qualidade dos assets existentes; o layout não cria automaticamente a arte pintada da referência.
- Layout de toque e nova sessão online não foram conferidos neste bloco.
