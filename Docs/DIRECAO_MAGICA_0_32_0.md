# Direção artística e correções — 0.32.0 (07/11/2026)

## Direção confirmada pelo titular
RPG regional é o jogo principal. Fantasia mágica/mística; natureza integrada à arquitetura, formas arredondadas, pedra trabalhada, madeira, metais antigos e cristais. Tecnologias antigas devem parecer encantamentos/relíquias: evitar consoles, painéis eletrônicos, laboratório moderno, faixas luminosas retas. A referência de inventário orienta materiais e ilustração; preservar layout e funções existentes.
A cidade precisa ser ampliada com vias para futuras montarias, áreas sociais, serviços, eventos e expansões. Não considerar a praça atual cidade final. Expansão será composição de bairros e vias, sem aumentar a escala dos personagens ou esticar construções.

## Entregue neste primeiro bloco
Configurações reais no RPG, acessíveis no topo e por Esc: volume geral, música, ambiente, efeitos, qualidade e diagnóstico FPS. Preferências existentes reutilizadas/salvas; sem timeScale/pause global. Inventário e configurações se fecham mutuamente, Esc possui um único responsável. Interação F bloqueada com interface aberta. Diagnóstico desconsidera frames das configurações.
Piso: união dos caminhos em células sem polígonos sobrepostos; elimina disputa de profundidade nas junções. Canteiros antigos que cruzavam os caminhos ocultados, fonte original preservada. Quatro bancos reposicionados para áreas laterais, navegação recalculada.
Protetor com aparência de console ocultado na cena, fonte de cristal existente colocada no lugar com colisão. Fonte é apresentação, não cria serviço/função falsa. Materiais/fonte antigos permanecem recuperáveis.
Grama recebeu textura nova pintada gerada pelo image_gen integrado, mipmaps, compressão, limite 1024, repetição e filtragem trilinear. Arquivo ativo: Assets/ForjaDasMares/Art/World/Coast/GrassFantasy032.png. Prompt de produção: textura albedo de grama curta para RPG fantástico pintado, vista superior, repetível, trevos discretos, sem objetos, iluminação neutra e detalhes definidos (não ruído verde desfocado). A ferramenta gerou a imagem; não foi modelada em Blender. Repetição visual ainda sujeita a avaliação artística.

## Arquivos
Novos: Networking/World/RegionalSettingsMenu.cs; Networking/Editor/TownVisualRepairInstaller.cs; Networking/Tests/RegionalSettingsTests.cs e TownVisualRepairTests.cs; GrassFantasy032.png; Data/World/Town/Surroundings/CityPathsClean.asset e metas.
Alterados sob Assets/ForjaDasMares: Networking/World/RegionalWorldHud.cs; Networking/CoopInventoryHud.cs e CoopInventoryHud.Regional.cs; Networking/CampaignPerformanceHud.cs; Networking/Editor/CampaignInstaller.cs; Scenes/RefugioUltimoSino.unity; Data/World/Town/Navigation.asset (binário recalculado pela Unity); Data/World/Town/Surroundings/CityMeadow.mat. ProjectSettings/ProjectSettings.asset: versão.

## Evidências e testes
51/51 PlayMode aprovados: inventário, configurações, cidade, arte, serviços/navegação, saída, reparo visual. Rodada inicial detectou conflito Esc, corrigido na rodada final. Ajuste posterior no filtro de coleta FPS compilado na build final (sem repetir a rodada inteira).
Build Windows Succeeded/errors=0/exit0, Logs/town032-windows.log. Mesmo executável estável Builds/Atual/Windows/ForjaDasMaresCampanha.exe. Cidade e painel observados na build; captura settings.png. Sem novo APK, Android físico ou multiplayer real. Não mede GPU ou garante FPS universal.

## Para testar
Abrir RPG → CONFIGURAÇÕES no topo → volumes, gráficos, FPS → VOLTAR/Esc. Abrir mochila e pressionar Esc: deve fechar sem abrir configurações simultaneamente. Conferir caminhos, bancos laterais, nova grama e fonte no centro.

## Ainda pendente do pedido visual
- Ampliação substancial da cidade e novos bairros/vias para montarias.
- Reformulação artística dos ícones/itens e acabamento dos painéis, mantendo layout.
- Substituir outros elementos angulares/futuristas e reconstruir arquitetura com formas orgânicas/ornamentos mágicos.
- Revisão de transições grama/pedra e cor geral com aprovação visual do titular.
Este bloco corrige problemas pontuais e adiciona configurações. Não representa equivalência visual às imagens conceituais nem arte final.
