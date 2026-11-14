# Clareza e funcionamento — 0.16.0

## Implementação
- Inventário de PC: categorias abreviadas sem quebra, pequenos símbolos de slot, contagem explícita dos itens filtrados, indicação de incompatibilidade em cor, instrução de uso derivada do ataque real, nome longo acessível na rolagem e comparação fixa do mesmo slot.
- Layout de toque: identidade separada (nome/nível/ouro/XP), barra de experiência e distribuição dos controles existentes; sem tentar comprimir três colunas no celular.
- Arcanista/projéteis: mira validada acompanhada até o instante de lançamento, direção de voo fixa após o lançamento; plano de mira acompanha a altura do jogador. Mira automática não procura além do alcance configurado.
- Apresentação: classe mágica sem arma selecionada mantém postura de cajado; pés giram gradualmente com o corpo ao virar parado. Não foram criados novos clips ou modelos finais.
- Interface vetorial: componentes CanvasRenderer explicitamente exigidos por ícones, superfícies arredondadas e minimapa, eliminando exceções comprovadas ao ocultar telas roláveis.
- Ajuda contextual dispensável: seis objetivos observados no jogo (movimento, combate, esquiva, recompensa, equipamento e talentos). Pode ser reaberta nas configurações, sem conceder itens ou pontos. Preferência por identidade local; não altera save v2.
- Encontros: aviso mínimo configurável de 0,6s e distância mínima configurável de 1,8m de um jogador no spawn. Reutiliza sinal visual/pool e sincronização existentes, sem aumentar vida/dano arbitrariamente. Mínimo pode ser ajustado no Inspector; geometria/limites continuam validados.
- Orientação: minimapa mostra quantidade de inimigos ativos e distância do mais próximo. Não representa cálculo de rota nem aponta um caminho seguro através de paredes.
- Desempenho: painel local opcional F8/configurações com FPS, p95 de tempo de quadro, memória Unity, inimigos, projéteis e tamanho do último snapshot. Nenhuma alegação de medição separada de CPU/GPU.
- Qualidade móvel: econômico limita a 30fps; demais modos visam 60fps; distância de sombras reduzida por qualidade na cópia de runtime. Metas não garantem desempenho em todo aparelho.
- Evolução online documentada em ONLINE_EVOLUCAO_0_16_0.md, sem substituir autoridade nem inventar infraestrutura.

## Verificação
- 76/76 testes PlayMode aprovados: Prisma, inventário, locomoção, progressão cooperativa, apresentação da campanha, armas e seleção de alvo. XML `Logs/clarity-0160-verified.xml`, saída0.
- Após os ajustes finais de apresentação/ajuda, 31/31 testes de interface aprovados, XML `Logs/clarity-0160-ui-final.xml`, saída0. Não somar como 107 testes únicos: são conjuntos sobrepostos.
- Rodadas iniciais encontraram a dependência CanvasRenderer ausente e um teste que pressupunha o modal antigo sempre ativo. A dependência foi corrigida e o teste passou a conferir o painel lateral real, incluindo nomes longos.
- Conferência manual no PC: equipar retirou item da mochila e alterou atributos; desequipar restaurou quantidade/atributos. Tela de toque a900x506 exibiu cabeçalho/XP, quatro itens, comparação e ações reais; um rodapé sobreposto foi ajustado.
- O diagnóstico abriu pelo menu. Amostra observada no PC:46FPS, p9529,4ms,118MB de memória Unity e snapshot4738bytes, no modo econômico com outras instâncias/aplicações em execução. É uma amostra sem carga representativa de horda, não benchmark final nem medição separada de CPU/GPU.
- Sessões de desenvolvimento usaram cópias/perfis de QA em `Logs/Review0160`, sem gastar itens, ouro ou talentos do jogador. O guia não persiste preferências durante `-runTests`.
- Conferência online detalhada em `REDE_VALIDACAO_0_16_0.md`.
- Build final Windows Succeeded/errors0/exit0, log `Logs/clarity-0160-delivery.log`; executável no caminho estável Builds/Atual/Windows/ForjaDasMaresCampanha.exe.

## Limites que permanecem
- Não há celular conectado. Layout de toque conferível no PC não é um teste físico Android.
- Não foi gerado outro APK neste bloco. O arquivo Android entregue anteriormente permanece 0.15.9/código40; o código futuro foi preparado para 0.16.0/código41.
- Teste com seu irmão e medições prolongadas no celular dependem dos participantes/aparelho.
- Polimento de animação e balanceamento não são considerados perfeitos por compilar. Requerem observação de sessões e ajustes graduais.
- Sem novos personagens, equipamentos, biomas, monetização ou serviços neste bloco.

## Cobertura da lista aprovada
1. Inventário PC: implementado, conferido e com regressão automatizada.
2. Inventário de toque: implementado/conferido em janela reduzida; aparelho pendente.
3. Identificação: símbolos de slot, incompatibilidade, requisitos e explicações melhorados; não há novas ilustrações únicas para cada item.
4. Comparações: fórmula real preservada e verificada; sem nota artificial de "melhor item".
5. Arcanista: correção de mira/lançamento e testes em oito direções aprovados.
6. Animação: postura de conjuração e rotação dos pés ajustadas; polimento visual humano permanece iterativo.
7. Primeiros passos: implementados, dispensáveis e reabríveis nas configurações.
8. Balanceamento: limites auditados e entrada de inimigos ajustada; sessões humanas medidas pendentes.
9. Orientação: minimapa com contagem/distância, aviso e afastamento de spawn; não há novo cálculo de caminho.
10. Cooperativo: dois PC reais pela internet/Relay conferidos; não representa teste com o irmão nem Android desta versão.
11. Evolução online: plano incremental documentado; nenhum servidor dedicado foi implantado.
12. Desempenho: diagnóstico e ajustes de qualidade implementados; medição prolongada/CPU/GPU/aparelho pendentes.

## Arquivos criados ou alterados
- `Assets/ForjaDasMares/Networking/CampaignCharacterSheet.cs`
- `Assets/ForjaDasMares/Networking/CampaignFirstSteps.cs`
- `Assets/ForjaDasMares/Networking/CampaignFirstSteps.cs.meta`
- `Assets/ForjaDasMares/Networking/CampaignHud.cs`
- `Assets/ForjaDasMares/Networking/CampaignMinimap.cs`
- `Assets/ForjaDasMares/Networking/CampaignPerformanceHud.cs`
- `Assets/ForjaDasMares/Networking/CampaignPerformanceHud.cs.meta`
- `Assets/ForjaDasMares/Networking/CampaignSessionMenu.cs`
- `Assets/ForjaDasMares/Networking/CoopDirector.cs`
- `Assets/ForjaDasMares/Networking/CoopInventoryHud.Desktop.cs`
- `Assets/ForjaDasMares/Networking/CoopInventoryHud.Touch.cs`
- `Assets/ForjaDasMares/Networking/CoopInventoryHud.Touch.cs.meta`
- `Assets/ForjaDasMares/Networking/CoopInventoryHud.cs`
- `Assets/ForjaDasMares/Networking/Editor/CampaignInstaller.cs`
- `Assets/ForjaDasMares/Networking/Tests/InventoryClarityTests.cs`
- `Assets/ForjaDasMares/Networking/Tests/PrismaTests.cs`
- `Assets/ForjaDasMares/Scripts/Gameplay/EnemyActor.cs`
- `Assets/ForjaDasMares/Scripts/Gameplay/EnemyArrivalVisual.cs`
- `Assets/ForjaDasMares/Scripts/Gameplay/EnemyPool.cs`
- `Assets/ForjaDasMares/Scripts/Gameplay/HeroCombat.cs`
- `Assets/ForjaDasMares/Scripts/Presentation/BrasaFootPlant.cs`
- `Assets/ForjaDasMares/Scripts/Presentation/HeroPresentation.cs`
- `Assets/ForjaDasMares/Scripts/UI/HudIcon.cs`
- `Assets/ForjaDasMares/Scripts/UI/ItemDescriptions.cs`
- `Assets/ForjaDasMares/Scripts/UI/PresentationQuality.cs`
- `Assets/ForjaDasMares/Scripts/UI/RoundedHudGraphic.cs`
- `Docs/BALANCEAMENTO_0_16_0.md`
- `Docs/CLAREZA_0_16_0.md`
- `Docs/INVENTARIO_LAYOUT_0_15_9.md`
- `Docs/MELHORIAS_PRIORIZADAS_0_15_9.md`
- `Docs/ONLINE_EVOLUCAO_0_16_0.md`
- `Docs/PROGRESSO.md`
- `Docs/REDE_VALIDACAO_0_16_0.md`
- `ProjectSettings/ProjectSettings.asset`
