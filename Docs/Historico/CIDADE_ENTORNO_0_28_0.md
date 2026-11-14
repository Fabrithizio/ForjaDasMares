# Cidade e entorno — 0.28.0

06/11/2026. Bloco restrito à cidade existente. Direção: praça de encontro, natureza ao redor de tecnologia antiga, acesso terrestre aos territórios e pátio de expedições interno. A imagem conceitual é referência de direção, não reprodução já alcançada.

## Implementado
- Migração da cena RefugioUltimoSino, preservando as oito pessoas provisórias, serviços, missão antiga, personagens e salvamentos.
- Composição de monumento antigo atrás da praça de chegada usando o kit existente. É representação visual; contaminação, proteção portátil e falhas do protetor não foram programadas.
- Pátio de expedições dentro da cidade, junto à tenda existente. Interagir com sua placa abre a seleção/lobby real CampaignCoop. Não há nova regra de cartas ou novo backend neste bloco.
- Saída sul substituída por estrada atravessável entre marcos e muralhas. Atravessar o limite aciona uma única troca para CostaAto1, com breve escurecimento. Clique no chão e joystick usam a mesma passagem; F continua disponível perto do destino.
- Não é streaming contínuo: o mapa de destino e seu ponto de chegada permanecem existentes. Continuidade completa dos dois lados e caminho de retorno serão tratados quando a região seguinte entrar no escopo.
- Terreno visual de 440 x 440 m, colinas, vegetação externa, serras simplificadas distantes e neblina gradual escondem as bordas da área acessível.
- Jardins localizados quebram parte do piso contínuo; pavimentação existente preservada após corrigir ampliação indevida de textura durante QA.
- Muralhas com coroamento/ameias, combinadas em sete malhas, com colisões simples. Árvores externas usam LOD e não lançam sombras; cenário distante não usa colisores, scripts de atualização ou luzes novas.
- Corrigido aviso legado de desafios perto da estrada: cidade anuncia apenas interações realmente disponíveis.

## Arquivos
- Networking/Editor/TownLayoutInstaller.cs: migração repetível da cidade, criação das malhas/material e validação de caminhos antes de salvar navegação.
- Networking/World/TownRoadPassage.cs: passagem física, só aceita o personagem configurado.
- Networking/World/TownHub.cs: transição com bloqueio de comandos/duplicação, fade e acesso ao lobby existente; dicas coerentes.
- Networking/Tests/TownLayoutTests.cs: caminhos, cobertura da câmera, travessia e expedições.
- Networking/Editor/TownArtInstaller.cs: reparo direcionado da navegação da costa, sem reconstrução visual.
- Networking/World/RegionalProbe.cs: posições opcionais de inspeção para build de desenvolvimento.
- Networking/Editor/CampaignInstaller.cs e ProjectSettings: versão Windows 0.28.0; código Android 51 reservado para futura compilação solicitada.
- Scenes/RefugioUltimoSino.unity; Data/World/Town/Navigation.asset; Data/World/Town/Surroundings/*.
- Data/World/Act1/Navigation.asset: reparado. Os dois arquivos binários de navegação foram danificados pela normalização de texto da entrega anterior. Restaurados de commit válido e recalculados contra a geometria existente. Não normalizar arquivos binários com ReadAllText/WriteAllText.

## Como conferir no PC
1. Abrir Builds/Atual/Windows/ForjaDasMaresCampanha.exe e entrar na cidade pela opção existente.
2. Percorrer praça, fonte, mercado, oficina e área de treino; conferir que serviços continuam acessíveis.
3. Ir à saída sul e clicar na estrada além do portão: caminhar pelo vão deve carregar a costa, sem aparelho para ativar.
4. Voltar à cidade pelo menu existente; ir à placa de expedições dentro da cidade e interagir para abrir o lobby/seleção dos desafios atuais.
5. Aproximar-se das muralhas e usar zoom máximo; conferir vegetação/terreno além delas, sem vazio. Testar também janela larga e estreita.
6. F8/indicador de FPS existente pode ajudar a localizar queda; não confundir a medição com garantia de desempenho móvel.

## Validação e limites
Resultados finais em Docs/Validation/0.28.0/summary.json. Não somar rodadas repetidas como testes únicos.
Build produzida somente para Windows no caminho estável. Nenhum APK gerado e nenhum teste físico Android ou sessão online executado nesta etapa. Compatibilidade de código não equivale a validação em aparelho.
Captura oculta inicial ficou preta e seus contadores de FPS foram descartados. Conferência visual usa execução visível; áudio desativado apenas nessa sessão QA (-noaudio), não no executável normal.
Não foram criados NPCs finais, casas com interiores, comércio novo, novas missões, itens, animações ou sistema de instâncias públicas. A cidade ainda é prévia local. Densidade, materiais e arquitetura precisam de novas revisões com o jogador; não é arte final no nível da referência.


Resultado final: 45/45 testes passaram (Logs/town028-verified.xml); build Windows concluída com errors=0 (Logs/town028-release-windows.log). Conferência visual real em Logs/Town028DeliveryQA. Capturas gate.png/edge.png preservadas junto ao relatório. Persistem ajustes estéticos, incluindo junção da pavimentação da estrada; não impedem a passagem.
