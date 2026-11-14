# Inventário — comparação automática 0.15.5

## Alterações
- Selecionar um item compara somente com o equipado no mesmo slot do herói atual.
- Referência recalculada quando o perfil ou equipamento muda; sem alternância para peças soltas da mochila.
- Ao inspecionar um item já equipado na secundária, a referência é a secundária real.
- Slot vazio indicado explicitamente; sem referência substituta da mochila.
- Cartões EQUIPADO ATUALMENTE / SELECIONADO com ícones e detalhes roláveis.
- Resumo separado das alterações efetivas da ficha, com números, setas e cores; prévia sem bônus temporários.
- Fundo ilustrado já existente e painéis da mochila/herói, mantendo o avatar e as regras atuais.
- Entregas continuam exigindo receber antes de equipar; prévia não concede itens.

## Arquivos
- Assets/ForjaDasMares/Networking/CoopInventoryHud.cs
- Assets/ForjaDasMares/Scripts/UI/ItemDescriptions.cs
- Assets/ForjaDasMares/Networking/Editor/CampaignInstaller.cs
- ProjectSettings/ProjectSettings.asset (versão)

## Como conferir
Abra a campanha e Personagem (ou I durante a partida). Selecione uma arma: o painel esquerdo deve mostrar a arma equipada. Selecione outra: a referência continua sendo a equipada. Inspecione armadura/acessório/artefato com slot vazio: deve dizer SLOT VAZIO. Equipar e voltar à mochila deve atualizar a referência. Ganhos e perdas usam números calculados pela progressão, não valores das imagens de referência.

## Limites
Esta etapa reorganiza a inspeção/comparação e a apresentação da mochila; não reproduz integralmente as ilustrações fornecidas nem adiciona capacetes/botas ou outros slots fictícios. Modelos, ícones e regras de equipamento existentes preservados. Valores e efeitos vêm do catálogo real. Resumo rápido apresenta até quatro alterações; detalhes completos e cartas de item têm rolagem. Nenhuma mudança no protocolo ou formato de save. Não foram executados novos testes automatizados nesta etapa.

## Verificação
Windows: build final Succeeded/errors=0, processo exit 0 (Logs/inventory-0155-pc.log). Conferência manual no executável 0.15.5: seleção de Corta-Maré rara e incomum manteve o Machado das Fendas equipado como referência; detalhes avançados acessíveis; seleção de Manto da Brasa Fria para Patrulheiro sem armadura exibiu SLOT VAZIO e prévia +20 vida/+12% defesa, sem referência da mochila. Navegação e troca de herói preservadas. Não foram alterados itens/ouro durante a conferência. Nenhuma das exceções NullReferenceException/MissingReferenceException/ArgumentException foi encontrada no Player.log desta execução. Android: build Succeeded/errors=0/exit0 (Logs/inventory-0155-android.log), APK real 0.15.5/código36 no caminho estável. Instalação USB no moto g15 via install -r: Success; dumpsys confirmou versão0.15.5/código36. Mesma aplicação, sem desinstalar/limpar dados. Interação física desta interface ainda não conferida.
