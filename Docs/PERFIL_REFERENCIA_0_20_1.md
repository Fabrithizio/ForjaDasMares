# Perfil e referência visual — 0.20.1

A correção segue a solicitação de manter recursos visíveis e equipamentos junto ao avatar. Ouro agora aparece numa barra fixa no cabeçalho de Personagem, Mochila, Status e Talentos, usando o saldo real de channel.LocalProfile. Foram mantidas quatro opções inferiores; não é necessário abrir uma tela de Recursos para ver o saldo. Não foram criadas moedas fictícias com base nas imagens.

A página Personagem reutiliza RoutesBackdrop com mais luminosidade. Os cinco slots reais ficam nos dois lados do modelo, com ícones, silhuetas para espaços vazios e borda de raridade. Clicar continua abrindo a inspeção funcional. A coluna direita mostra nome, nível, XP e seis status com símbolos e valores. A ficha detalhada mantém explicações e ganha ícones por atributo.

Transparência da câmera de prévia: pós-processamento local desativado na prévia transparente, pois a configuração URP descartava alfa. Antialiasing de quatro amostras definido ao criar o RenderTexture do corpo, sem recriá-lo durante Configure. Nenhuma alteração global do pipeline para esta finalidade. Ícone de moeda desenhado no sistema HudIcon existente, enum acrescentado ao final para conservar índices serializados.

Verificação: 14/14 InventoryClarityTests aprovados via -force-d3d11, com asserções de barra visível na mochila e personagem, saldo real e quatro botões inferiores. Build Windows 0.20.1 concluída errors=0. Conferência visual de Personagem e Mochila em execução real: Logs/ProfileReferenceQA/character.png e probe/screen.png. Perfil QA isolado, sem mudar o save normal. Log do player sem NullReference/MissingReference/Exception.

O primeiro teste do Editor em D3D12 encerrou por Unrecoverable GPU device error na GPU/driver local. Preservado em Logs/profile-reference-tests.log. A execução D3D11 subsequente passou, registrada em profile-reference-tests-dx11.log e profile-reference-tests.xml. A build/player Windows local utiliza D3D11. Não foram atualizados drivers ou configurações de segurança do computador.

Como testar: abrir Personagem no início, conferir cenário/ícones e clicar num equipamento; entrar na Mochila e observar o saldo no topo; reciclar com confirmação e conferir atualização do ouro; abrir Status e Talentos sem perder a carteira. Equipagem não altera a quantidade de moedas. Telas comparativas continuam usando o mesmo slot equipado.

Arquivos: Networking/CoopInventoryHud.Areas.cs, Scripts/UI/EquipmentPaperDoll.cs, Scripts/UI/HudIcon.cs, Networking/Tests/InventoryClarityTests.cs, Networking/Editor/CampaignInstaller.cs, ProjectSettings/ProjectSettings.asset e Docs/PROGRESSO.md. Nenhum novo download/asset de terceiros, alteração de combate, economia ou slots. Layout de toque equivalente e desempenho físico móvel continuam pendentes; sem APK.
