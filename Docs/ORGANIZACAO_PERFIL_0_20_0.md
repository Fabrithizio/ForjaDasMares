# Organização do perfil — 0.20.0

## Entrega
No PC, cinco destinos próprios pela barra inferior: Personagem, Mochila, Status, Talentos e Recursos. O botão Personagem da tela inicial abre a visão do herói; o atalho I continua abrindo a mochila. A navegação não compra talentos nem altera itens/ouro.

Personagem: modelo real em quadro próprio, classe/nome, nível e barra de experiência, papel, pontos fortes/fracos, resumo, troca de herói quando permitida e cinco slots existentes com ícones. Clicar no slot abre sua inspeção na mochila. Nenhum novo slot de equipamento foi introduzido.
Mochila: grade ampliada, filtros por slot/classe, capacidade compartilhada, mochila/saque/entregas, recebimento em lote existente e comparação automática com o mesmo slot. Equipar/desequipar/reciclar continuam usando as transações reais do canal de perfil. Não foram adicionados consumíveis fictícios.
Status: página independente, rolável, com Combate, Sobrevivência e Mobilidade/Habilidades. Nome, valor e explicação separados. Mostra dano/base/crítico/cadência/elemental; vida/defesa/redução/regeneração/bloqueio; movimento/recarga/velocidade de ataque/alcance/esquiva. Usa ProgressionStats.Calculate, sem fórmulas de dano duplicadas nem mudança de balanceamento. Ficha permanente: buffs temporários de combate ficam explicitamente fora da apresentação. Exemplo da redução de defesa usa MitigatedDamage real. Novas categorias/linhas podem ser acrescentadas sem alterar o inventário ou persistência.
Talentos: painel funcional existente preservado, com acesso próprio e dimensões ajustadas para manter a barra inferior disponível.
Recursos: ouro e contagens reais de mochila, saque e entregas; acesso a entregas e ao perfil da última sala quando disponível. Não cria carteira global online, moedas premium, materiais ou economia nova.

## Verificação
14/14 InventoryClarityTests aprovados. Novo teste percorre Personagem/Status/Recursos/Mochila, verifica cinco slots e os status de defesa, confere retorno ao Personagem ao fechar Talentos e compara o JSON antes/depois para detectar mutação indevida. Teste antigo de entrega atualizado para aceitar a inspeção lateral real do desktop além do modal de toque.
Build Windows 0.20.0 concluída com errors=0. Conferência visual: consultar Logs/profile-areas-windows.log, Logs/profile-areas-player.log e capturas em Logs/ProfileAreasQA/probe. Foram conferidas telas reais de mochila, personagem e status usando perfil QA isolado; cabeçalho e fundo corrigidos após a primeira inspeção. Perfil normal do usuário preservado.

## Como conferir
Abra a versão estável em Builds/Atual/Windows/ForjaDasMaresCampanha.exe. No início clique Personagem e use a barra inferior. Confira status nas três categorias; role a lista. Na mochila selecione uma peça, compare e equipe; no Personagem clique no slot e desequipe. Em Recursos abra Entregas; Talentos continua exigindo pontos reais e confirmação para redistribuir.
Sem APK nem teste físico Android. Layout de toque existente preservado; esta reorganização visual é para desktop. Adaptação equivalente da navegação em celular permanece pendente para a fase móvel, sem alegar paridade visual. Novas moedas, slots, habilidades e resistências das imagens são referências de direção, não recursos implementados.

## Arquivos
Novo: Networking/CoopInventoryHud.Areas.cs e meta.
Alterados: CoopInventoryHud.cs, CoopInventoryHud.Desktop.cs, CampaignCharacterSheet.cs, CampaignHud.cs, CoopSmokeProbe.cs (comandos de QA), Tests/InventoryClarityTests.cs, Scripts/UI/EquipmentPaperDoll.cs, Editor/CampaignInstaller.cs, ProjectSettings/ProjectSettings.asset, Docs/PROGRESSO.md.
Arte: MenuArt/RoutesBackdrop existente reutilizado. Ícones reais de equipamento e símbolos do tema; não houve downloads ou novos assets externos. Estrutura atual conserva cinco slots de dados e as regras de persistência/rede.
