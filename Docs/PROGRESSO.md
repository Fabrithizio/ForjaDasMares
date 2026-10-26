# Atualização0.16.0 — clareza, mira, interface e verificação cooperativa

Inventário PC/toque reorganizado, identificação e descrições melhoradas, mira acompanhada até lançamento, postura/pés ajustados, guia contextual dispensável, spawn com aviso/afastamento e diagnóstico local de desempenho.76/76 testes aprovados, seguidos por31/31 testes de interface sobrepostos após ajustes finais. Build final Windows Succeeded/errors0/exit0. Duas instâncias Windows pelo Relay: mapa, ataque remoto, retorno após queda, morte/resgate, resultado, próxima fase e encerramento conferidos. Vitória/derrota administrativas não equivalem a uma partida humana completa.

Conhecido: reconexão direta local após encerramento abrupto falhou; saída normal direta e retorno abrupto Relay funcionaram. Código móvel preparado, sem APK0.16.0 ou teste físico. Balanceamento humano, celular, redes distintas e quatro jogadores pendentes. [Implementação/cobertura](CLAREZA_0_16_0.md), [rede/evidências](REDE_VALIDACAO_0_16_0.md), [balanceamento](BALANCEAMENTO_0_16_0.md), [evolução online](ONLINE_EVOLUCAO_0_16_0.md).

---

# Atualização 0.15.9 — inventário de PC em três colunas

Mochila à esquerda, identidade e personagem real ao centro, detalhes/comparação automática e ações à direita. Nome, nível, ouro e XP passaram a elementos separados, com barra de progresso; 16 itens por página e slots compactos. Funções e dados existentes preservados. Compilação Windows Succeeded/errors0/exit0 e início0.15.9 aberto; revisão visual por cliques ainda pendente, interrompida por interação na janela. Sem novo teste automatizado/online, sem gerar APK. [Arquivos, passos e limitações](INVENTARIO_LAYOUT_0_15_9.md).

---

# Atualização 0.15.8 — limite visível, receber todos e tela de talentos

Capacidade quantidade/limite e aviso CHEIA; recebimento em lote transacional e validado, com preservação do excedente. Talentos no estilo da quinta referência: herói real, lista rolável, ícones coloridos, detalhe atual/próximo, compra e redistribuição reais. Metadados visuais opcionais para arte futura; conteúdo continua com seis talentos. Savev2 preservado; protocolo11 exige versões compatíveis em todos os clientes.

Primeira build Windows Succeeded/errors0/exit0. Conferência em perfis isolados: recebimento total/parcial sem perda/duplicação, Vigor consumiu1 ponto e aumentou8 de vida, máximo bloqueado, rolagem e redistribuição com confirmação/custo correto. Build final Windows Succeeded/errors0/exit0. Android cancelado a pedido do usuário: a partir desta etapa, manter código móvel sem gerar APK/instalar/testar celular até solicitação explícita. Nova sessão online pendente. Nenhuma suíte automatizada nova executada. [Arquivos, evidências e limites](ENTREGAS_E_TALENTOS_0_15_8.md).

---

# Correção 0.15.7 — desequipar com mochila cheia

Removido bloqueio de capacidade em ProfileService.Unequip: peça permanente excedente vai para ENTREGAS; com espaço, permanece na mochila. Loot temporário mantém sua origem no saque. Instâncias são movidas sem cópias, com validação/transação de salvamento preservada; regra comum aos slots, inclusive retirada de principal/secundária.

Windows Succeeded/errors0/exit0. Verificado com o perfil real: arma saiu do slot, mochila36/Entregas1→2, item exato/atributos/total de IDs/ouro preservados. Reinício confirmou slot vazio e item salvo. Android Succeeded/errors0/exit0, APK0.15.7/código38 atualizado no moto g15 via install -r Success, versão confirmada e dados preservados; interação desta correção no celular não conferida. Expectativa do teste existente de mochila cheia atualizada; suíte automatizada e sessão online não executadas neste bloco. [Arquivos, passos e limites](DESEQUIPAR_0_15_7.md).

---

# Atualização 0.15.6 — sete melhorias visuais do inventário

Retratos/nomes/raridade destacados, tabela de oito atributos atual/ao equipar/diferença, blocos de ataque/efeito e resumo essencial sem rolagem. Ações por estado com botão principal destacado. Mochila com ícones maiores, bordas por raridade/seleção e incompatibilidade visível. Layout de toque com quatro itens por página e abas equipado/selecionado, mantendo a referência automática do slot. Novo componente de apresentação separado dos pedidos de equipamento; sem alteração de combate, economia, protocolo10 ou savev2.

Builds finais Windows/Android Succeeded/errors0/exit0. Conferidos no PC mochila, comparação, equipamento atual e Avançado; no moto g15, layout 2x2, paginação, abas com a mesma prévia e bloqueio de classe incompatível. APK final0.15.6/código37 instalado com install -r Success e versão confirmada; mesma aplicação/dados preservados. Reaberto e conferido no aparelho após a atualização final. Não foram executados novos testes automatizados nem uma nova sessão online neste bloco de interface. [Arquivos, passos e limites](INVENTARIO_VISUAL_0_15_6.md).

---

# Atualização 0.15.5 — inventário com comparação automática

Inspeção compara exclusivamente com o item equipado no mesmo slot do herói; slot vazio explícito, sem referência alternativa da mochila. Cartões lado a lado, ícones, resumo de ganhos/perdas e detalhes roláveis. Fundo ilustrado existente e painéis de mochila/herói reorganizados. Regras de equipamento, saves v2 e protocolo10 preservados.

Windows compilado Succeeded/errors0/exit0; conferência manual de duas seleções de arma contra a mesma referência equipada, detalhes avançados e armadura com slot vazio. Android compilado Succeeded/errors0/exit0, APK 0.15.5/código36 no caminho estável. Atualizado no moto g15 via install -r (Success), versão0.15.5/código36 confirmada; dados preservados. Teste físico de toque desta interface pendente. Não foram executados novos testes automatizados. [Arquivos, verificação e limites](INVENTARIO_COMPARACAO_0_15_5.md).

---

# Atualização 0.15.4 — apresentação e entrada na campanha

Artes originais integradas ao início e campanha; seleção de herói por cartões, entrada solo/privada/grupo/código, grupo do lobby, conexão/carregamento/cancelamento e avisos do host organizados. Bordas da interface suavizadas; antialiasing da cena configurado por plataforma, com três modos gráficos. **99/99 testes finais aprovados, Unity exit0.** Builds finais Windows e Android Succeeded/errors0/exit0. Solo, lobby e dois clientes Windows reais pelo Relay conferidos; saída/retorno do cliente, encerramento do host e criação de grupo aberto verificados. APK0.15.4/código35 gerado. **Atualização em 24/10: moto g15 instalado por cima (Success), versão0.15.4/código35 conferida.** Conferidos início/campanha/heróis por toque; PC–Android reais pelo Relay com mesmo mapa, dois heróis, escolha de altar e movimento do joystick validado no host; encerramento do host retornou o Android ao início. Sessão curta com saves isolados; app devolvido ao perfil normal. Sem teste entre redes diferentes ou quatro pessoas. Resultados na evidência de entrega. [Arquivos e limites](APRESENTACAO_0_15_4.md). [Artes e prompts](ARTES_MENU_0_15_4.md).

---

# Atualização 0.15.3 — mochila e itens mais claros

Abas MOCHILA/SAQUE/ENTREGAS, categorias e filtros legíveis, slots com ícones e função, resumo do item com classe/nível/slot/dano/bônus, comparação pela fórmula real e detalhes avançados opcionais. Recebimento aguarda propriedade confirmada; reciclagem mantém confirmação explícita. Perfil local e cópia da sala identificados. **56/56 testes finais aprovados, exit0**; conferência visual/equipagem Windows em perfil isolado. Builds Windows e Android Succeeded/errors0/exit0. moto g15 atualizado por cima para 0.15.3/código34; conferidos mochila, comparação e equipagem por toque em perfil isolado. Aplicativo devolvido ao modo normal. Sem nova sessão de rede neste bloco. [Arquivos, teste e limites](INVENTARIO_0_15_3.md).

---

# Atualização 0.15.2 — mira e disparos do Arcanista

Corrigidos o uso de direção antiga ao começar um ataque entre pacotes e a orientação visual/replicada dos projéteis. Cada conjuração mantém sua direção, com movimento livre; a próxima usa a última mira válida recebida. Mira automática móvel agora retém alvos com critérios de troca, visão e alcance configuráveis. Sem mudanças de dano ou de salvamento.

**Verificado:** duas falhas reproduzidas antes da correção; **30/30 testes PlayMode aprovados depois**, Unity exit0. Build Windows Succeeded/errors0/exit0, versão0.15.2 aberta e conferida na tela inicial. Build Android Succeeded/errors0/exit0; APK 0.15.2/código33 conferido no manifesto, no caminho estável Builds/Atual/Android/ForjaDasMaresCampanha.apk. moto g15 reconhecido ao final e atualizado via install -r (Success); versão0.15.2/código33 conferida. Teste de jogabilidade físico ainda pendente. Nova sessão real PC–Android não executada neste bloco. [Relatório, arquivos e testes](MIRA_0_15_2.md). Itens/mochila e fluxo de menus permanecem para os próximos blocos.

---

# Atualização 0.15.1 — primeiro bloco de clareza: equipagem

Instalada primeiro a 0.15.0 solicitada no moto g15 e depois atualizada a mesma instalação para 0.15.1/código32, preservando dados. Corrigido bloqueio de equipagem por efeitos de outros atores; condições do host centralizadas com motivos; resposta de recusa retornada ao solicitante e aviso no painel de comparação. Mantidas regras de classe/nível/slot e saves v2.

**Verificado:** 8/8 testes PlayMode de progressão/equipagem aprovados; builds Windows e Android Succeeded errors0, exit0. Instalação USB Success e versão do pacote conferida. Ainda faltam teste visual e sessão real com dois jogadores desta correção. Cliente pode mostrar botão antes de saber que um efeito próprio continua ativo no host; autoridade recusa com motivo. Mira do Arcanista, reorganização geral da mochila e menus continuam pendentes: não foram alteradas neste bloco. [Relatório e arquivos](EQUIPAGEM_0_15_1.md).

---

# Revisão de clareza e funcionalidade — análise

Identificadas divergências entre permissão de equipagem na interface e no host, bloqueio por projéteis/áreas do pool compartilhado e recusa sem motivo visível. Mira do Arcanista exige reproduzir divergência entre direção guardada do ataque, emissor móvel e pose. Plano em blocos, sem implementação nesta revisão: [análise e critérios](ANALISE_CLAREZA_FUNCIONALIDADE_0_15.md). Não foram executados novos testes ou alteradas builds/saves.

---

# Atualização 0.15.0 — Prisma, o Arcanista

Modelo original do mago com 25 clipes reais, manto/rig, animações de cajado, grimório e orbe, efeitos/sons e encaixes de arma configurados nos dados. Mantidos ataque segurado, pesado, habilidades, Convergência, equipamentos/talentos, arenas e jogo cooperativo. Corrigidos perfil sonoro por herói, indicador do super e relógio da animação de esquiva remota. Foco de Brasa preserva sua identidade.

**Testar:** use o atalho habitual em Jogos e escolha a terceira classe, ARCANISTA — Prisma. Segure ataque; E/habilidade; Espaço/esquiva; Q/super cheio; I/Personagem. Builds atuais: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk (0.15.0/código31). Mesmo pacote Android, protocolo10 e saves v2; não desinstalar para atualizar.

57/57 testes finais focados; duas instâncias Windows reais pelo Relay conferidas. Consolidação por caso: 231 aprovados/0 falhas/1 ignorado, não uma única execução geral final. Builds Windows e Android reais, errors0/exit0. Teste físico Android desta versão pendente: celular ausente no USB. Arte/equilíbrio ainda de protótipo. [Relatório, arquivos e limitações](PRISMA_0_15_0.md). Histórico abaixo.

---

# Atualização 0.14.2 — HUD mais limpo

Retirados os botões fixos AQUI/ALVO/PERIGO/SAQUE do PC e celular. APK atual: Builds/Atual/Android/ForjaDasMaresCampanha.apk (0.14.2/código30); envie como Documento no WhatsApp. PC: atalho existente Jogar Campanha Forja das Mares. Protocolo10/savev2 preservados; atualizar por cima, sem apagar dados.

24/24 testes focados aprovados; builds Windows e Android reais. [Relatório desta correção](REVISAO_0_14_2.md). Histórico abaixo.

---

# Revisão atual 0.14.1 — 21/10/2026

Base funcional dos 20 pontos integrada anteriormente, agora com marcações separadas, diagnóstico de entrada corrigido e proteção do movimento contra frames longos/quedas acidentais. Suíte geral:218 aprovados,0 falhas,1 ignorado (219 casos); reexecução final de segurança/interface:25/25. Quatro clientes Windows pelo Relay passaram na conferência de mapa, bênçãos, marcações, piso sem resgates, baú único, resultado administrativo e encerramento do host. Não foi teste com quatro pessoas.

Builds reais 0.14.1: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk (código 29). Use o atalho existente Jogos/Jogar Campanha Forja das Mares. Mesmo pacote Android e saves preservados; moto g15 atualizado e conferido com o PC pelo Relay no mesmo Wi-Fi, incluindo joystick, marcações e reconexão. Sem nova pasta de envio ao irmão.

[Relatório atual e limitações](REVISAO_0_14_1_2026-10-21.md) · [evidências](Evidencias_0_14_1/testes.json). Aprovado com limitações para testes privados no PC e neste Android; arte, equilíbrio humano, medições móveis e lançamento comercial pendentes. Histórico abaixo.

---

# Revisão atual 0.14.0 — testes privados

Arsenal original com ícones e12 novos clipes de ataque da Brasa; slots de equipamento com imagens; efeitos e sinergias de armas; esquiva perfeita; altares com escolhas; esconderijos; desafios de ondas/proteção/escolta; barris, parede quebrável e ponte frágil; marcações cooperativas e novos sons originais. A base dos20 pontos aprovados está integrada; modelos de cenário e heróis ainda são provisórios, e os demais heróis não receberam animações finais novas.

**Abrir:** Jogos/Jogar Campanha Forja das Mares. Saídas estáveis: Builds/Atual/Windows/ForjaDasMaresCampanha.exe e Builds/Atual/Android/ForjaDasMaresCampanha.apk. Android usa o mesmo pacote com.forjadasmares.campaign (versão28). Protocolo10 exige todos atualizados. Saves preservados; não apagar a instalação atual. A pasta antiga de envio ao irmão não foi atualizada.

Validação:69/69 testes na reexecução final, após corrigir13 falhas de validação detectadas na suíte geral. União do resultado mais recente por caso:214 aprovados/1 ignorado, não uma única execução geral final. Dois processos Windows pelo Relay conferidos, incluindo bênçãos, marcações, retorno, baú único e encerramento do host. Vitória de diagnóstico foi administrativa. Build Windows e APK Android reais; teste físico desta revisão pendente porque o moto g15 não está no USB. Não testado entre redes diferentes ou por quatro pessoas.

[Como jogar](CAMPANHA_COMO_JOGAR.md) · [revisão e limitações](REVISAO_0_14_2026-10-20.md) · [arquivos](ARQUIVOS_0_14.txt) · [evidências](Evidencias_0_14/testes.json).

Aprovado com limitações para testes privados. Sem conta global, migração de host ou servidor dedicado. Arte final, balanceamento humano prolongado, outros dispositivos e medições de desempenho continuam pendentes. Histórico abaixo.

---

# Iteração 0.12 — aprovada no PC com limitações

Implementados: equipamentos fora da grade da mochila; seis talentos por herói; atributos em cartões; detalhes individuais e comparação entre itens; reciclagem confirmada por ouro; XP/ouro imediato por morte; efeitos de drop com pool; retrato no HUD; minimapa menor à esquerda; objetivo compacto. Catálogo migrado na Unity sem erros de compilação. Suíte geral184/183 aprovados/0 falhas/1 ignorado; reexecução de rede75/74/0/1; rodada final RPG11/11. Build Windows0.12.0 produzida sem erros. Dois clientes PC pelo Relay, persistência após reiniciar, equipagem e modais conferidos; resultados no documento REVISAO_0_12_2026-10-18.md.

Preservados saves, cenas, pacote de rede, perfil por host, combos, Fúria e controles existentes. Não foi solicitada nova distribuição ao irmão; pasta de compartilhamento e APK anterior preservados. Android físico desta revisão ainda não validado. Sem conta global, servidor dedicado ou migração de host.

---

# Entrega atual — 0.11.0, 18/10/2026

Minimapa com inimigos, setor pendente e objetivos; reforços intermediários sem apagar remanescentes; emissores de flechas e avisos de chegada; arma inicial comum focada, sem bônus solo; armas por classe com combos/habilidades reais; inventário por slots/categorias, secundária permitida e comparação; personagem acessível no início; barra de habilidades no PC com ícones e Fúria temporária do guerreiro. Saves mantidos e loadouts antigos incompatíveis desequipados para a mochila. Sem modelos/clips finais novos nesta revisão.

Validação: regressão PlayMode geral com 174 casos, 173 aprovados, zero falhas, 1 ignorado; grupo focado de correções: 14/14 aprovados. Após o último ajuste visual da réplica, repetidos todos os testes de rede: 65 casos, 64 aprovados, 0 falhas, 1 ignorado. Unity 6.3 LTS (6000.3.26f1). Builds reais Windows/Android 0.11.0, protocolo 7, APK ARM64 versão 25. Teste físico PC + moto g15 no mesmo Wi-Fi pelo Relay: entrada, movimento, mapa compartilhado, derrota→lobby→fase seguinte→reentrada, intenção offline validada no host e encerramento sincronizado. Resultado/recompensas na rodada anterior foram acionados administrativamente, não por vitória humana. Não testado entre redes diferentes, quatro pessoas ou FPS medido.

**Aprovado com limitações para testes privados.** Host ainda encerra a sala quando sai; sem migração/servidor dedicado. Perfil deste aparelho e cache da última sala são contextos separados, não uma conta global. Arte, animações, equilíbrio humano, sessões prolongadas e desempenho em outros dispositivos ainda exigem trabalho. Não é release comercial/PlayStore.

Arquitetura atualizada em ARQUITETURA.md; configurações/passos em CAMPANHA_COMO_JOGAR.md; arquivos em ARQUIVOS_0_11.txt. Dependências, evidências e problemas intermediários em REVISAO_0_11_2026-10-18.md. Fonte/dados/documentação entram no GitHub; saves pessoais e builds não são inventário global nem backup de conta. Os registros abaixo são históricos.

---

# Entrega atual — campanha 0.9.1, 17/10/2026

Base existente preservada; esta revisão melhora estabilidade, exploração e combate cooperativo. Foram implementados suprimentos, selos opcionais, fontes limitadas, encontros variantes, três armas e carregado/SUPER do guerreiro com autoridade do host. Inventário e resultado receberam correções reais; protocolo5, sem migração de save. Consulte `AVENTURA_0_9_2026-10-17.md` para evidências, lista de arquivos, builds e limites. Instruções atuais em `CAMPANHA_COMO_JOGAR.md`; os registros abaixo são históricos.

Não declarar lançamento comercial concluído: arte/animação de todos os heróis, equilíbrio humano, testes prolongados, outras redes e celulares continuam pendentes. Sem migração de host/servidor dedicado.

Validação final:151 testes PlayMode,150 aprovados,0 falhas,1 ignorado; builds Windows/Android0.9.1 e teste físico PC–moto g15 pelo Relay. Sessão4/4 anterior deste bloco:3 processosWindows+1Android, não quatro pessoas.

---

# Etapa 3 — entrega atual (13/10/2026)

Quatro heróis configurados e selecionáveis na ArenaTest existente. Guerreiro: espada em arco/combo e impacto; Patrulheiro: projéteis e armadilha; Arcanista: gelo/fogo/raio e pulso glacial; Cavaleiro Rúnico: armadura, proteção do aliado e investida. ScriptableObjects separados; pool de projéteis/zonas; dano, facção, direção, obstáculos, knockback, recuperação/cooldowns e invulnerabilidade finita. Sem equipamento ou multiplayer.

Joystick Android substituído por base fixa, indicador interno e captura de pointerId. Testes verificaram retenção do vetor, liberação de outro dedo e deslocamento do CharacterController. O moto g15 voltou a ser detectado e o APK foi instalado e testado nele por entrada de toque injetada via ADB. Movimento segurado, ataques e habilidades dos quatro heróis, esquiva e reset conferidos em capturas. Avaliação humana de ergonomia e multitoque simultâneo ainda é necessária.

Verificação final: **23 testes PlayMode, 23 aprovados, zero falhas**; testes-delivery.xml. Mantidos os 11 testes da base e adicionados 12 de heróis/efeitos/controles/apresentação, incluindo reset da cadeia e liberação da direção ao trocar herói. Primeiro corrigida a criação de MaterialPropertyBlock no construtor. Depois corrigidos colisão do aliado com o percurso de teste e raio da explosão de fogo. A rodada final inclui movimento pelo joystick, investida/dano único/knockback e poses de dano/morte/vitória.

Não há clips reais ou modelos finais. Idle/Walk/Attack/Skill/Dodge/Hit/Dead/Victory são estados de placeholders procedurais, com pontos de integração de Animator documentados. Animation integrado 1.0.0 habilitado. VS Code continua o editor; Visual Studio IDE não foi instalado.

Guia: Docs/HEROIS_COMO_TESTAR.md. Catálogo: Docs/ARQUIVOS_ETAPA3.txt. Próxima etapa de inimigos depende de nova instrução. Histórico abaixo descreve checkpoints anteriores.

---
# Progresso — Forja das Marés

## Entrega atual

Etapas 1 e 2: projeto auditado/configurado e primeira arena jogável concluída. Alvos atuais: Windows e Android; iOS adiado.

Unity 6.3 LTS 6000.3.26f1, URP 17.3.0, Input System 1.14.2 e uGUI 2.0.0. A cena real salva é `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, incluída nas configurações de build. A licença do Editor foi validada. VS Code e extensões C#/Unity instalados; Visual Studio IDE não foi instalado.

## Implementado

- Arena com chão, limites, obstáculos, luz, spawn seguro e placeholders geométricos originais.
- Movimento em oito direções, diagonal normalizada, aceleração/desaceleração, colisão com CharacterController e esquiva.
- Entrada separada pelo Input System: teclado/mouse, bindings de gamepad e controles mínimos de toque.
- Câmera diagonal suave, enquadramento por proporção de tela e tentativa de evitar obstáculos por inclinação/colisão.
- Ataque próximo com alcance/arco, preparação/recuperação, bloqueio por parede e dano sem duplicação por collider.
- Alvo com vida, flash de acerto, barra no HUD e reset da arena por R/botão.
- Dado de arma em ScriptableObject, dependências explícitas na cena e componentes separados para apresentação/lógica/entrada.

## Verificação executada

O Editor importou o projeto, compilou os assemblies e gerou a cena com `ARENA_BUILD_OK`. PlayMode: **11 testes executados, 11 aprovados, zero falhas**, registrados em `arena-test-results.xml` na entrega. Foram verificados movimento/diagonal, variação de taxa de quadros, paredes/esquiva, obstáculos, dano/cooldown/reset, bloqueio de golpe, enquadramento, componentes/referências, entrada real do Input System com dispositivos sintéticos, acompanhamento suave e controles virtuais de toque.

A primeira execução apresentou uma falha de entrada de teclado sintético em batch sem foco de Game. O teste passou a configurar temporariamente o encaminhamento de entrada do Editor para testes sem foco e restaurar a configuração depois. A segunda execução completa aprovou os 11 testes. Isso não representa uma sessão física com teclado/gamepad ou telefone Android.

## Plano por etapa

| Etapa | Estado |
| --- | --- |
| 1. Auditoria/arquitetura | Concluída para esta base local; arquitetura futura documentada |
| 2. Arena | Concluída e validada no Editor por 11 testes de PlayMode |
| 3. Heróis/habilidades | Não iniciada |
| 4. Inimigos | Não iniciada |
| 5. Salas/masmorras | Não iniciada |
| 6. Progressão/saques | Não iniciada |
| 7. Multiplayer | Não iniciada; revalidar NGO/Transport/serviços antes de integrar |
| 8. Interface/controles completos | Não iniciada; somente HUD/controles mínimos da arena |
| 9. Conteúdo | Não iniciada |
| 10. Builds/validação em aparelhos | Não iniciada; testes necessários acompanham cada entrega |

## Arquivos criados e alterados

Criados: template oficial Universal 3D em Assets/Packages/ProjectSettings, `.gitignore`, pasta `Assets/ForjaDasMares` com cena, materiais, arma, input actions, assemblies, componentes, ferramentas de geração/captura e testes; `Docs/ARENA_COMO_JOGAR.md` e catálogo `Docs/ARQUIVOS_ETAPA2.txt`.

Alterados: os quatro documentos anteriores (`LEIA-ME.md`, `Docs/AUDITORIA.md`, `Docs/ARQUITETURA.md`, `Docs/PROGRESSO.md`); manifest/lock; configurações de versão, player, layers, render pipeline e build scenes do template. Os arquivos do jogo estão catalogados em `ARQUIVOS_ETAPA2.txt`.

## Limitações e dependências restantes

Não há multiplayer, progressão, inventário, inimigos ou conteúdo comercial nesta entrega. Os placeholders não são modelos/animações finais. Foi gerado APK ARM64 e realizado teste funcional no moto g15. Ainda faltam a matriz de aparelhos, profiling e validação de distribuição; não há crossplay implementado ou comprovado. Perfil de desempenho e ajustes de toque/safe area/acessibilidade completos precisam de testes em dispositivos. Não há áudio nesta arena. A câmera foi validada em obstáculos baixos desta cena; salas fechadas precisam de nova avaliação.

Na etapa 7 serão necessários projeto UGS, autenticação/serviços e validação real de sessões. iOS permanece fora do escopo atual. Backend/economia e autoridade comercial serão definidos nas etapas correspondentes.

## Executar e próxima etapa

Abrir o projeto pelo Unity Hub, abrir `Assets/ForjaDasMares/Scenes/ArenaTest.unity`, clicar Play e dar foco à janela Game. WASD/setas movem, mouse mira, clique esquerdo ataca, Espaço esquiva e R reinicia. Guia detalhado: `ARENA_COMO_JOGAR.md`.

Aguardar a próxima instrução do usuário antes de iniciar heróis/habilidades ou outros sistemas.

## Conferência visual

Captura da cena real em PlayMode gerada e inspecionada: chão/limites, luz/sombras, jogador ciano com espada geométrica, alvo laranja, obstáculos e HUD visíveis. Foi corrigida a renderização de cores desativando GPU Resident Drawer/SRP Batcher nos perfis desta arena. Essas otimizações devem ser reavaliadas em profiling. A limitação de indexação interna do Editor encontrada na primeira captura está documentada em AUDITORIA.md.

## Ambiente instalado e conferido

Unity Hub, Unity 6.3 LTS 6000.3.26f1, VS Code com C#/C# Dev Kit/Unity/.NET Install Tool e Git instalados. A solução do projeto foi gerada para o editor de código. Não foi instalado Visual Studio IDE.

Android Build Support, OpenJDK 17.0.18+8, NDK r27c (27.2.12479018), CMake 3.22.1, SDK Build Tools 36.0.0, Platform Tools 37.0.1, plataformas 34/35/36/37 e Command Line Tools 16.0 instalados. A verificação oficial retornou sucesso e status ok para todos os 13 componentes. Java e adb executaram corretamente; source.properties confirmou a versão do NDK. Nenhum telefone foi encontrado por adb devices.

O suporte Android foi instalado primeiro, com sucesso. A chamada seguinte para instalar os filhos também tentou repetir o módulo pai e reportou falha nessa repetição; todos os 11 filhos foram instalados. A verificação posterior confirmou o pai e os filhos presentes, sem erro. Esse resultado não substitui compilação de APK.

Referência de integração: https://code.visualstudio.com/docs/other/unity . O pacote com.unity.ide.visualstudio 2.0.25 atende à integração de VS Code; não representa instalação do Visual Studio IDE.

Tudo necessário para desenvolver e executar esta arena no Editor está disponível. Gradle pode baixar componentes na primeira compilação Android. Builds Windows IL2CPP exigirão ferramentas C++ específicas na etapa de builds. Agora há executável Windows e APK Android de desenvolvimento; consultar a validação adicional abaixo.

## Correção e validação no PC — 13/10/2026

O usuário abriu a cena vazia Untitled do template e tentou Play. Foi acrescentado ArenaLauncher: abre a arena na inicialização quando existe somente a cena padrão vazia e sem alterações; oferece F6/menu e redireciona Play da cena padrão vazia, preservando a confirmação de salvar cenas alteradas. Não substitui cenas com conteúdo próprio.

O módulo de áudio foi explicitamente incluído no manifest para impedir a remoção do AudioListener; física também passou a ser dependência direta. O teste de referências passou a exigir AudioListener na câmera. 11 testes de PlayMode foram executados novamente e todos passaram.

ArenaBuilds foi acrescentado para compilar a cena explícita: Windows x64/Mono e Android ARM64/IL2CPP. A versão Windows foi compilada com sucesso, executada e observada na tela. O usuário confirmou movimento WASD, ataque e reset funcionando. Executável local: Builds/Windows/ForjaDasMares.exe. Android: moto g15 detectado, API 35 e somente arm64-v8a; APK gerado e teste funcional concluído, como registrado abaixo.

Arquivos novos desta correção: README.md, Editor/ArenaLauncher.cs e Editor/ArenaBuilds.cs (com seus meta files). Alterados: Packages/manifest.json e lock, teste ArenaAcceptanceTests, configurações de plataforma necessárias, documentação de progresso. Nenhum sistema da próxima etapa foi iniciado.

## Backup remoto confirmado

Repositório: https://github.com/Fabrithizio/ForjaDasMares . O usuário autorizou explicitamente a publicação no repositório público após ser informado da visibilidade.

Checkpoint Windows validado enviado pela CLI Git à branch main: commit 20468aac96053e173fd6a8e5c734ac305140f675. Foi feita uma nova clonagem do GitHub, independente do checkout original: 155 arquivos e árvore e45ce78dee18226b6614d332299f8efcd9c440f4, exatamente iguais ao checkpoint local. Assets, meta files, Packages, ProjectSettings e documentação estão no backup. Caches, builds e metadados temporários de testes não estão incluídos.

A conexão GitHub do aplicativo recusou a primeira tentativa de escrita (HTTP 403, Resource not accessible by integration). O envio foi concluído pelo Git local com autenticação via Git Credential Manager. Nenhum segredo foi incluído no projeto.

Há também um ZIP local do código-fonte na entrega de validação. Builds executáveis ficam localmente em Builds; para recuperar o trabalho em outro PC, clonar o repositório e abrir com Unity 6000.3.26f1. Este registro descreve um checkpoint confirmado, não uma automação de backups futuros.

## Validação Android concluída — 13/10/2026

APK de desenvolvimento 0.1.0, pacote com.forjadasmares.prototype, IL2CPP/arm64-v8a, target API 35, mínimo API 25. O Gradle 9.3.1 do Editor retornou BUILD SUCCESSFUL para assembleDebug. APK instalado pelo adb com Success e aberto na atividade UnityPlayerActivity do moto g15 / Android 15. O app usa o perfil Mobile.

Teste real via entrada de toque injetada no aparelho conectado: joystick deslocou o personagem do spawn até o alvo; ataque reduziu a vida de 100 para 75; esquiva deslocou o personagem; reiniciar restaurou vida 100 e posição inicial. Capturas de cada estado foram conferidas. A consulta de logs filtrada pelo processo do jogo não encontrou Exception, NullReference, FATAL ou Error na sessão consultada. O jogo foi deixado aberto e reiniciado no celular.

Este é um teste funcional curto em um aparelho, não um benchmark, teste de longa duração, teste humano de ergonomia ou comprovação de compatibilidade com todos os Androids. Não há multiplayer/crossplay nesta entrega.

### Limitação de montagem Android nesta sessão

O pipeline Unity concluiu shaders e os 544 objetos nativos ARM64, mas sua invocação interna do Gradle falhou no Java NIO com Unable to establish loopback connection / UnixDomainSockets Invalid argument: connect. Foram ensaiadas preferências IPv4, seletor alternativo e opções de ambiente da Unity; as tentativas completas pela Unity ainda falharam no pós-processamento, sem erro C#.

A montagem real foi concluída executando diretamente o Gradle no projeto Android gerado em Library/Bee/Android/Prj/IL2CPP/Gradle. Um teste Java independente confirmou Selector.open funcionando quando o JDK usa seu fallback TCP. A opção jdk.net.unixdomain.tmpdir foi direcionada somente nesse processo a um caminho indisponível para sockets Unix; a implementação do PipeImpl do JDK instalado trata a falha do listener e usa TCP. Nenhuma configuração global de Windows, segurança ou firewall foi alterada.

O APK testado foi copiado do launcher-debug.apk produzido com BUILD SUCCESSFUL. O menu Build Android prototype usa o pipeline padrão Unity; sua montagem integral precisa ser revalidada no Editor aberto normalmente fora desta sessão. Não foi mantida uma tentativa de contornar o problema por alteração do ambiente dentro do C#, pois ela não se propagou à invocação Java da Unity.

Avisos observados: metadados SDK em Program Files não puderam ser regravados, e opções geradas pela Unity foram marcadas como deprecated pelo Gradle. A montagem direta terminou com sucesso apesar desses avisos. Reavaliar SDK gravável/atualizações na próxima preparação de builds, sem alterar permissões de segurança do sistema.

### Arquivos e builds

Criados nesta rodada: README.md, ArenaLauncher.cs e ArenaBuilds.cs com meta files, checkpoint Git e ZIP do projeto. Atualizados: manifest/lock para áudio/física, teste de referências, configurações Android ARM64/IL2CPP/API35, guia, progresso e auditoria. O helper de build inclui menus Windows/Android; a ferramenta de abertura oferece F6 e protege cenas alteradas.

Windows: Builds/Windows/ForjaDasMares.exe, compilado e confirmado pelo usuário. Android: Builds/Android/ForjaDasMares.apk, gerado, instalado e testado. Builds e capturas não ficam no Git; os arquivos que permitem reconstruir o projeto ficam no repositório. O novo checkpoint deve incluir estas conclusões.

Verificação final após os ajustes dos menus e da proteção de cenas: 11 testes PlayMode executados novamente, 11 aprovados, zero falhas; código C# compilado. O helper de abertura também preserva cenas com componentes personalizados. Nenhuma próxima etapa foi iniciada.


## Builds da etapa 3

Windows 0.2.0 x64/Mono: BuildPipeline retornou ARENA_PLAYER_BUILD_OK e saída 0. Executável iniciado nesta rodada; inicializou PhysX/Input System e carregou a cena, sem Exception, NullReference, MissingReference ou FATAL no log consultado. Encerrado o processo de teste criado por esta sessão. Não foi uma nova confirmação humana de teclado/mouse no executável; os comandos foram verificados nos testes PlayMode.

Captura real URP da arena atual gerada por ArenaPreviewCapture e inspecionada: chão/obstáculos/iluminação/câmera, guerreiro com espada geométrica, grupo de alvos, aliado e HUD presentes. Arquivo arena-herois.png na entrega. ZIP do executável e todos os dados Windows produzido.

Android 0.2.0: montagem em andamento neste checkpoint. Nova confirmação física depende do aparelho voltar a ser detectado por ADB. O código funcional no PC pode ser salvo no GitHub conforme autorização anterior.

### Nova rodada no aparelho

O moto g15 voltou a ser detectado. A primeira montagem 0.2.0 terminou no Gradle em 45 s e instalou com Success, mas o teste físico revelou stripping de SphereCollider chamado por GameObject.CreatePrimitive no pool. Este APK inicial não é a entrega final. Pool corrigido para criar MeshFilter/MeshRenderer com malhas explícitas previamente serializadas na cena, sem criar/remover colliders. Verificações e montagem estão sendo repetidas.

## Conclusão da etapa 3 — versão 0.2.0

Última suíte: 23 testes PlayMode aprovados, zero falhas, após a correção do pool Android e do reset do combo/trava de direção. O pool usa malhas serializadas com MeshFilter/MeshRenderer e não cria colliders no runtime. O stripping identificado no primeiro APK foi corrigido.

Android final: IL2CPP/ARM64, mínimo API 25, alvo API 35, versionCode 2, versionName 0.2.0. Montagem direta Gradle concluída em 22 s com BUILD SUCCESSFUL; instalada com Success e aberta no moto g15. Nova verificação da versão final: joystick segurado desloca o herói, primeiro ataque deixa alvo em 74, reset restaura alvo e reinicia combo (primeiro golpe novamente 74). Log de 52 linhas do novo processo, sem E Unity, Exception, NullReference, MissingReference ou FATAL na consulta. Perfil Mobile confirmado pelo diagnóstico de runtime. Capturas reais conferidas.

Sessão física desta etapa, com toque injetado no aparelho: Guerreiro 100→74→29 (golpe e impacto/empurrão de grupo); Patrulheiro projétil 100→80 e armadilha 100→72 com alvos imobilizados; Arcanista 100→84→64→46 (gelo/fogo/raio), pulso 100→68 com imobilização de grupo; Cavaleiro golpe+investida 100→54, proteção reduz dano no aliado 150→134 e no herói 210→196, depois expira (segundo dano: aliado 94, herói 163). Esquiva desloca o personagem e reset restaura arena/vida/câmera. Cada resultado observado em captura, além dos testes automatizados.

A proteção só começa após a preparação da habilidade; pressionar DANO imediatamente, antes da resolução, aplica o dano normal. Foi repetido o teste aguardando a preparação, comprovando redução e expiração. Não é invulnerabilidade permanente.

Limitações: placeholders geométricos e poses procedurais; gamepad físico, ergonomia humana/multitoque simultâneo, profiling, outros aparelhos e testes de longa duração ainda pendentes. A chamada interna do Gradle pela Unity continua falhando nesta máquina em Java NIO; o APK entregue usa montagem direta do projeto gerado, com ajuste somente no processo Java. Sem alteração de segurança/firewall/global do Windows. Não há equipamento, progressão, IA de inimigos ou multiplayer/crossplay nesta etapa.

O projeto permanece em Área de Trabalho/Jogos/ForjaDasMares. Abrir a cena por F6 e Play. A próxima etapa exige nova instrução do usuário.

SHA256 do APK entregue: 0493543A863A925F13627778B15D9D01FC9DD36F2616B71E899A72A9CAD3E092

Windows final recompilado com ARENA_PLAYER_BUILD_OK e saída 0. Executável final iniciado, ARENA_RUNTIME_READY confirmou PC_RPAsset e referências da arena; consulta do log sem exceções. Processo de teste encerrado e ZIP atualizado. Backup final inclui a correção Android e os 23 testes.

## Etapa 4 — integração PvE em validação final (13/10/2026)

Arena preservada; cinco arquétipos, minion de invocação e chefe Custódio da Forja com três ataques/fase 2. AI Navigation 2.0.16 e módulo AI resolvidos e compilados; dois NavMeshData salvos, raio do chefe 0,8. IA separada de movimento, dano e apresentação; CharacterController mantém colisões. Pool fixo de 24 atores, limite 12 em Android e 18 em PC, limite de invocação por dono e orçamento de sessão. Armas/ataques/vida/controle existentes reutilizados. Avisos geométricos e três bipes sintetizados originais, sem arte/clips finais.

HUD permite escolher encontro e iniciar. R cancela e limpa; troca de herói cancela o desafio da arena. Ondas aguardam a fila de spawn e os inimigos relevantes; derrota encerra o encontro. Chefe dá 25 selos temporários uma vez, sem inventário ou salvamento. Projetil termina no primeiro impacto; dano em área deduplica Health; morte limpa invocações, projéteis e avisos.

Primeira suíte: 33/33 PlayMode aprovados, incluindo os 23 anteriores, quatro heróis × cinco arquétipos e quatro heróis contra o chefe, morte, navegação ao redor da parede, bloqueio, áudio configurado, dano e limpeza. Duas verificações extras de ondas e colliders múltiplos foram acrescentadas e aguardam execução final. Build PC e captura ainda em andamento neste registro.

Android sem aparelho nesta etapa a pedido do usuário. Nenhuma confirmação humana, teste físico de celular, profiling de desempenho ou balanceamento comercial é alegado. Etapa 5 está autorizada, mas só começa após finalizar a validação da etapa 4. Multiplayer/equipamentos continuam fora do escopo.

## Fechamento PvE

35/35 testes PlayMode aprovados, zero falhas. As duas verificações adicionais confirmaram exatamente nove spawns nas três ondas e um único dano em área para o jogador com colliders múltiplos. O executável Windows 0.3.0 compilou, abriu com ARENA_RUNTIME_READY e PC_RPAsset, sem exceções no log. Captura URP real com três inimigos ativos revisada; a sobreposição do texto do encontro foi corrigida no HUD. Build final será recompilado com esse ajuste visual.

Etapa 4 validada no PC. Não houve teste físico Android nesta etapa; o telefone foi levado pelo usuário. Não há multiplayer, inventário ou arte final. Etapa 5 pode começar conforme autorização explícita de executar na ordem, sem confirmação presencial.

## Etapa 5 — masmorras em verificação final

Reutilizada a arena/PvE validada no checkpoint 7869a62. Nova DungeonTest, sem regravar ArenaTest: SHA256 da referência e da cena preservada 07D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4. Três biomas geométricos originais com materiais/luzes/decoração e tabelas diferentes; oito categorias de sala; layout seeded com percurso conectado e ramificações, 7–12 salas; modo fixo.

Portas físicas/carving fecham em encontro e abrem após vitória. Evento exige cristal; armadilha tem aviso/bipe e alavanca; chave da sessão abre tesouro opcional; baú abre e concede moedas uma vez; descanso cura uma vez; saída exige chefe vencido. Câmara com limite opcional usado somente na masmorra. NavMesh é construído e validado para inimigos normais e chefe; spawn valida geometria/colisão. Sem aumento arbitrário de atributos, equipamentos ou multiplayer.

Os testes individuais já passaram para 2.000 seeds × seis limites (12.000 combinações), mesma seed igual assinatura, 12 gerações reais em três biomas/quatro seeds, portas, objetivos, baú, reset e percurso completo com movimento/colisões/combate real do Cavaleiro Rúnico, descanso, três encontros, chefe e saída. Corrigido reset de alvos de referência inativos. Bot de teste usa NavMeshQueryFilter explícito do agente normal para evitar consultas ambíguas entre duas superfícies. A suíte completa e builds ainda estão em execução.

Android sem teste físico por instrução do usuário; preparar build para instalação posterior. Arte/clips/profiling/gamepad físico permanecem pendentes. A próxima etapa não será iniciada automaticamente.

Suíte completa da etapa 5: 41/41 PlayMode aprovados, zero falhas; mantém os 35 testes do PvE e acrescenta seis testes de masmorra. Percurso completo passou em 72,98 s no teste isolado e novamente na suíte completa. Seeds verificadas em 12.000 combinações lógicas; 12 gerações físicas com NavMesh de ambos os tamanhos. Builds/capturas seguem como validação separada, sem alegar Android físico.

Verificação adicional: câmera visível em 16:9,4:3 e9:16. A interação F foi exercitada pelo estado Dynamic do Input System, callback real e Update da sessão. O teste sem Game View focada precisou encaminhar o teclado sintético para o player; configurações de foco do teste são restauradas no finally e não alteram a configuração do jogo. Ajustado o raio da investida do chefe de0,9 para1,5 para superar a margem de contato dos colliders; aviso de linha agora cobre também a extensão final do golpe, e dano fica no semicírculo frontal. Suíte final de44 casos em execução, com caso novo para impacto único da investida.

Validação final do código: 44/44 testes PlayMode aprovados, zero falhas, incluindo a investida com dano real e único, F pelo Input System e câmera em três proporções. Os quatro heróis continuam vencendo cada arquétipo e o chefe com a correção de alcance. Percurso completo da masmorra aprovado novamente. Unity encerrou com resultado Passed. Capturas catacumbas/floresta revisadas; fortaleza/builds ainda em preparação.

Revisão final da interface: barra do chefe agora usa geometria real com largura proporcional à vida, pois Image Filled sem sprite não reduz o desenho. Helper reutilizado na arena e na masmorra, sem regravar ArenaTest. Teste verifica largura real, alinhamento e proporção. Suíte completa final: 45/45 aprovados, zero falhas. Windows0.4 compilou e abriu com DUNGEON_GENERATED/DUNGEON_RUNTIME_READY, sem exceções; recompilar para incluir o helper final. Android IL2CPP/URP compilou e gerou projeto Gradle, mas montagem interna encontrou Java NIO Unable to establish loopback connection, mesma limitação conhecida. Preparar montagem direta após recompilação final.

## Entrega final da etapa 5 — 13/10/2026

45/45 PlayMode aprovados. Windows0.4.0 final recompilado com o helper da barra, iniciado e DUNGEON_RUNTIME_READY/seed12345 confirmou geração de9 salas; log sem exceções. Processo de teste encerrado e ZIP atualizado. Atalho Jogar ForjaDasMares na pasta Área de Trabalho/Jogos aponta para Builds/Windows/ForjaDasMares.exe.

Android0.4.0 final: IL2CPP/URP/nativo gerados pela Unity; montagem interna falhou somente no problema conhecido Java NIO. Gradle direto terminou BUILD SUCCESSFUL em25s. APK copiado do launcher-debug.apk gerado, versãoCode4, minSDK25,targetSDK35,ARM64; assinatura v2 verificada pelo apksigner. Arquivo38707750 bytes. SHA25663803B4EFFFB7F1D8C8B4ABE206687EE0A14D23F4B84233614B8B88D7AC9614A. Esta versão não foi instalada/testada em aparelho: usuário levou o telefone. A compilação não prova funcionamento físico/crossplay.

Ajuste de Java limitado ao processo de montagem, sem alterações de firewall/segurança/ambiente global. Avisos de metadados SDK somente leitura e de opções Gradle obsoletas não impediram a montagem. O menu Android usa o pipeline padrão; nesta máquina a montagem direta permanece necessária nesta sessão.

Capturas URP reais dos três biomas revistas. ArenaTest preservada byte a byte, SHA25607D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4. Cenas/materiais/ScriptableObjects/prefabs/.meta/configurações estão no Git; Library/Builds ficam fora. Arquivo fonte ZIP e guias serão exportados do checkpoint final, com verificação em clone independente.

Limites: placeholders geométricos e poses, sem arte/clips finais; sem profiling de aparelhos, ergonomia humana, gamepad físico, equipamento completo, persistência completa, multiplayer/crossplay. Etapas4 e5 concluídas neste recorte; não avançar a equipamentos/rede sem próxima instrução. Desenvolvimento continua por blocos, com backup no GitHub após cada validação.

Backup concluído e verificado: checkpoint032e5e3a51f625e23282a21d9e0ee2b0d806d2d1 e árvore595b100317c2318b7ff7251041afa0497f417796 iguais em clone independente do GitHub e no projeto local. FonteZIP exportada por git archive. Entrega em outputs/ForjaDasMares-Etapa5: PCZIP,APK,fonteZIP,guia,progresso,verificação,recuperação,lista exata de105 arquivos criados/15 alterados desde o PvE (inclui.meta),XML45 e três capturas reais. Apenas documentos mudam após esse checkpoint para registrar a conferência; os binários e código testados permanecem os da validação45.

## Correção PC 0.4.1 — 13/10/2026

O teste humano revelou cenário preto e Development Console repetindo falta de recursos ScreenSpaceAmbientOcclusion. O registro anterior de inicialização do Windows 0.4.0 não comprovava a renderização sustentada do executável; as capturas eram do Editor e os 45 testes de lógica não detectaram este defeito gráfico. A validação anterior do executável fica corrigida por este registro. Não usar o ZIP de PC 0.4.0 para jogar.

Auditado o código instalado da URP 17.3: CoreBuildData e URPPreprocessBuild coletam recursos pelo EditorUserBuildSettings.activeBuildTarget; os strippers de SSAO dependem desses recursos. ArenaBuilds definia somente BuildPlayerOptions.target, permitindo gerar Windows após Android sem garantir o destino ativo. Agora o menu ativa o destino; em batch uma plataforma divergente interrompe a compilação com instrução explícita. Nova build iniciada com -buildTarget Win64, confirmação ARENA_BUILD_TARGET target=StandaloneWindows64 active=StandaloneWindows64 e ARENA_PLAYER_BUILD_OK, sem erros de compilação. SSAO continua ativo: a falha foi corrigida na geração dos recursos, sem esconder o console ou retirar efeitos para mascará-la.

Executável 0.4.1 observado diretamente na tela deste PC: catacumbas, floresta subterrânea e fortaleza vulcânica renderizadas; transição para ArenaTest; seleção de patrulheiro pelo botão; disparo de mouse reduziu alvo de 100 para 80; três inimigos perseguiram e causaram dano; reinício por botão restaurou vida e cancelou o encontro. Execução acompanhada por vários minutos, sem repetição SSAO ou exceções/LogError no log. A injeção de uma tecla pela ferramenta de controle não confirmou seleção por teclado; não alegamos um novo teste humano de WASD nesta correção. Os 45 testes anteriores não foram reexecutados nesta rodada: código runtime/cenas não foi alterado. ArenaTest conserva SHA256 07D9C27D59A80610BC2D7FE669ADA364A1CB961CB9FA177BEA33D28B7399A7C4.

Alterados somente ArenaBuilds.cs, ProjectSettings.asset (versão 0.4.1), README.md e este documento. PC ZIP atualizado em outputs/ForjaDasMares-Correcao-PC-0.4.1; atalho da pasta Jogos utiliza o executável corrigido. APK anterior continua 0.4.0 e não foi recompilado ou testado em aparelho nesta correção. Próxima compilação Android deve iniciar com -buildTarget Android. Nenhuma nova etapa de conteúdo foi iniciada.

## Etapa 6 — código de progressão em integração, 14/10/2026

Checkpoint intermediário, não entrega final: definidas instâncias de equipamento, raridades/afixos, fórmulas, regras de slots, serviço de perfil, save local e ponte de eventos. Ainda faltam catálogo real, cenas de composição, inventário/interface e validação integrada. A ordem autorizada é progressão → rede → interface completa → conteúdo → revisão de lançamento, com revisão técnica antes de avançar. O usuário autorizou continuar sem presença e fazer backup por bloco.

Auditoria inicial: sem nomes de scripts duplicados, métodos vazios ou TODO/FIXME críticos no código próprio. Runtime não importa UnityEditor; os três asmdefs próprios não formam ciclo. MeleeAttack/CombatFeedback antigos continuam referenciados pelo ArenaSession e instalador da base, mas ficam desativados nas cenas de heróis, preservando compatibilidade. Não apagar esses arquivos sem migrar referências. Rede/Lobby/Relay não existem no projeto; não alegar que estão integrados.

45/45 testes existentes passaram novamente com as primeiras extensões opcionais do combate. Os12 testes novos de domínio da progressão encontraram5 falhas: JsonUtility materializava campos nulos de classe como objetos vazios, impedindo equipagem e fazendo saves concluídos recuarem ao backup. Correção: clone preserva presença, envelope2 registra presença de expedição/resultado e envelope1 normaliza somente sentinelas vazias. Nova execução12/12 aprovada. Testes incluem escrita bloqueada com rollback, recuperação de backup/corrupção, migração e rejeição de versão futura, afixos, regras de secundária, comparação de duas mãos, descarte confirmado, estoque cheio e deduplicação após reload. Esses testes usam catálogo temporário; não comprovam inventário jogável nem reinício humano do executável.

Outras correções comprovadas nesta revisão: enum/NaN inválidos rejeitados pelo catálogo; removida dependência do validador de dados no cálculo de gameplay; comparação de arma de duas mãos retira bônus secundário; inimigo continua concedendo XP quando o saque temporário opcional está cheio; reserva para recompensas obrigatórias; OnDestroy da ponte ignora inicialização incompleta. ArenaTest e DungeonTest permanecem sem regravação. Compilação Unity disponível, não substituída por inspeção de texto. Prosseguir com integração da etapa6 antes da rede, conforme última autorização.

## Etapa6 — integração e PC validados, Android em preparação

Catálogo real com12 itens iniciais,9 categorias de armas,5 raridades,9 afixos limitados e tabelas de recompensa. Variantes ProgressionDungeon/ProgressionArena integram inventário, comparação, slots, entregas, descarte confirmado, resultado, opções e mascote. Equipamentos alteram os padrões reais; crítico/elemental/multiplicadores são capturados na ação e propagados a projéteis/zonas. Arma equipada acompanha as poses existentes; arte/animações continuam geométricas.

Suíte completa:68/68 PlayMode aprovados,0 falhas. Inclui todos45 anteriores,13 de domínio,9 de integração e percurso completo com equipamento/baú/chefe/saída/recompensas salvas. Auditoria de4 cenas/7 prefabs/92 definições/416 behaviours:0 scripts ausentes,0 referências obrigatórias/quebradas,0 problemas. A inspeção do executável encontrou rótulos estáticos vazios em botões; corrigidos e testados:9/9 integração repetida com assert dos textos,0 falhas.

Windows0.5.0 final compilado com destino ativo correto e ARENA_PLAYER_BUILD_OK. Executável observado diretamente: mapa, HUD, Lume, inventário paginado, textos dos botões, comparação e EQUIPAR funcionando. Nenhuma exceção/LogError/falta SSAO no log acompanhado. Probe em processos Windows separados: write abriu baú real, aplicou morte e gravou13 itens/5XP/5 moedas/armadura/opções; read após fechar/reabrir restaurou os mesmos dados, vida máxima160 e opções. Diretório de teste isolado; perfil normal do usuário preservado.

Build Android anterior foi cancelado para incluir o rótulo corrigido; versão final IL2CPP/ARM64 está compilando. Nenhum aparelho conectado em adb devices; teste físico/crossplay não realizado. Ainda não instalar pacotes de rede até finalizar este bloco. Etapas de rede/interface completa/conteúdo/lançamento permanecem na fila, por autorização do usuário de continuar enquanto dorme. Fonte intermediária anterior no GitHub79b21a8; novo backup após este bloco.

## Etapa6 — entrega final local0.5.0

Windows final0.5.0 conferido com cenário, inventário, rótulos, comparação e EQUIPAR reais; nenhum erro registrado no acompanhamento. ZIP68142673 bytes, SHA2569BDF8FA86B10954D9F9D88549A958C99B4B45A9FB816EC52B6EA639ED1AC8E16. Probe de reinício da aplicação passou:13 itens/5XP/5 moedas/armadura/vida máxima160/opções restaurados de arquivo em processos separados. Perfil de QA isolado; dados normais do usuário preservados. Atalho da pasta Jogos continua apontando para o executável atualizado.

Android final0.5.0: código/nativo IL2CPP e projeto Gradle gerados. Montagem interna falhou no Java NIO Unable to establish loopback connection; Gradle direto com ajuste restrito ao processo terminou BUILD SUCCESSFUL em45s. APK copiado do launcher-debug.apk gerado; package com.forjadasmares.prototype, versionCode5/versionName0.5.0,minSDK25,targetSDK35,ARM64; assinatura v2 verificada. SHA25627F3B367ED451CC08C68F057D71C8065D90832391F4CBA4C33990A863529E89D. Não instalado/testado em aparelho: adb não encontrou telefone.

A compilação Android removeu temporariamente os dois registros SSAO da lista ativa de UniversalRenderPipelineGlobalSettings; restauração da configuração canônica inspecionada após o Editor encerrar. Build Windows sempre ativa Win64 antes de coletar recursos. Não remover esses registros do fonte compartilhado: fonte de PC precisa deles. Não esconder falhas por desativar o Development Console.

Backup GitHub do bloco de código/cenas/dados/docs:73dfcb6731e62e752af6c5c81d28da323087e924. Entrega em outputs/ForjaDasMares-Etapa6: PCZIP,APK,fonteZIP,XML68,XML9 da revisão de rótulos,auditoriaJSON,logs de persistência,guia/revisão/lista de arquivos. Documentação de entrega será atualizada no próximo commit, sem mudança de código testado. Classificação: aprovado com limitações para este protótipo local; sem aprovação de lançamento, economia online, crossplay ou teste físico Android. Próxima etapa autorizada: auditoria/migração de rede; ainda não há pacotes/serviços configurados neste checkpoint.

## Etapa7 — bloco de sessão privada validado, implementação continua
Base72/72 e cena/protocolo6/6 PlayMode aprovados. Build Windows cooperativo separado gerado e executado:host+3 clientes, mesmo layout confirmado, quinto recusado, movimento/ataque remotos reais, morte do herói do host com equipe sobrevivente, saída/reconexão na mesma vaga e encerramento ao sair host. Logs da terceira execução sem exceções. Detalhes e limitações em REDE_SESSAO_VALIDACAO.md. Etapa7 ainda não concluída: faltam saques/progressão/reaparecimento completos, fluxo completo de PvE/chefe, medição/latência/QA visual. UGS implementado com guard de configuração, mas projeto não vinculado e internet/crossplay não testados. Interface completa e expansão seguem depois, sem pular etapas.

Revisão geral posterior ao bloco de sessão: suíte PlayMode completa78/78 aprovada,0 falhas, XML work/etapa7/regression-78.xml. Nenhuma compilação permanece em execução após essa suíte. A etapa7 continua parcial pelas limitações já documentadas.

Continuação do domínio de recompensas da etapa7: CoopRewardLedger e seis testes reais de persistência/deduplicação adicionados;6/6 aprovados em execução isolada. Ainda falta conectar o coordenador à sessão e replicar os perfis; nenhuma recompensa cooperativa integrada foi declarada pronta. Arquivos novos: Networking/CoopRewardLedger.cs e Networking/Tests/CoopRewardLedgerTests.cs, com metadados. Não houve mudança nas cenas, controles ou builds neste bloco.

Android físico0.5.0: moto g15 conectado, instalação-r preservando dados, movimento/ataque/habilidade/projétil/troca/inventário/equipagem e persistência após reinício observados no aparelho. Detalhes/limitações em ANDROID_TESTE_FISICO_2026-10-14.md. Rede PC–Android ainda não testada; preparação do APK cooperativo iniciada. Revisão do novo stick cooperativo encontrou pivot inferior esquerdo incompatível com cálculo central: ajustado para0.5,0.5 antes do build, sem alterar o stick local validado.

## Etapa7 — progressão integrada e LAN física, continuação14/10

PC e moto g15 conectados realmente2/4: mesmo mapa, movimento Android observado no host, início de encontro/sala2, vida/morte/derrota compartilhadas e Closed no celular ao encerrar o host. APK cooperativo inicial0.6.0 produzido/assinado/instalado. Esse primeiro teste não comprovou golpe remoto nem vitória: os heróis morreram enquanto ficaram sem lutar. Detalhes no documento Android. Internet/Relay continuam pendentes de vínculo UGS.

CoopProgression/CoopProfileChannel/CoopInventoryHud ligam perfis privados do host, saques reais, resultado e intents validados à sessão. Equipamentos aparecem nas replicas e modificam o combate da autoridade. GUID de reconexão persiste; morto reaparece após vitória de sala com50% de vida e ponto seguro; derrota/abandono finalizam os perfis. Health.Revive é explícito, Heal não ressuscita. HeroEquipment.Initialize remove assinaturas anteriores. Corrigido filtro de mouse da interface e reposicionados controles de sessão fora do joystick; revalidar na nova build física.

Suíte geral89/89 aprovada (zero falhas). Rede17/17 repetida após assertions de reload. Percurso automatizado da autoridade venceu combates/chefe/saída com ataques reais, concedeu/gravou resultado e rejeitou replay. Arquivos novos: três componentes CoopProgression/CoopProfileChannel/CoopInventoryHud e CoopProgressionTests, com.meta. Alterados: CoopActor/Director/Hud/Protocol/SmokeProbe/CoopInstaller/CoopProtocolTests, composição CoopDungeon, Health/HeroEquipment e documentação; versão de build será0.6.1. Compilação de builds Windows/Android em andamento; não afirmar ainda que esses binários novos foram testados fisicamente.

Windows cooperativo0.6.1 gerado com COOP_BUILD Succeeded/errors0. Quatro processos reais carregaram protocolo2/perfis privados; ataque remoto matou goblin e concedeu12XP a cada participante; baú real adicionou10XP, repetição retornouFalse sem crédito adicional. Derrota controlada pela instrumentação de QA finalizou cada perfil com13 itens/11XP. Após fechar/reabrir quatro processos, todos restauraram13 itens/11XP em Lobby; replicas receberam somente o perfil do seu próprio slot. Quinta conexão recusada; saída do host levou clientes aClosed/0 conectados. Provas em work/etapa7/four-final/proof. A derrota controlada e o baú acionado pelo harness não são uma vitória humana de quatro jogadores; o percurso com chefe continua comprovado pelo teste automatizado da autoridade.

Android0.6.1/code7 ainda em compilação nativa IL2CPP. Não declarar instalação nem teste físico desta revisão até produzir/verificar o APK. ArenaTest e DungeonTest conservam os SHA256 anteriores. Backup do código validado em Windows será feito agora; a validação física posterior será registrada em outro checkpoint.

### Continuação física concluída neste recorte

Backup do bloco validado:1b9f23f no GitHub. Android0.6.1/code7 real instalado e testado com Windows: ataque do telefone matou goblin, joystick moveu na autoridade, equipagem de adagas persistiu no host, reinício do cliente/reconexão em40s recuperou vaga/herói/vida/equipamento. Tentativa após90s foi recusada. Resultado de baú/derrota apareceu com item/XP/moedas reais no Android. Host rodou em segundo plano após ajuste de build. Reaparecimento0→70HP após vitória de sala observado no telefone; inimigos desse teste foram derrotados pelo harness, não por combate humano.

Correções finais: guardas no harness antes de usar mapa, apresentação do herói selecionado após Close e runInBackground na build Windows. O harness anterior gerou uma NullReferenceException ao receber comando no Lobby; nova rodada com a guarda não reproduziu.17/17 de rede aprovados novamente: network-guarded-17.xml. Fonte de gameplay offline preservado; suíte geral89/89 anterior permanece como regressão e a rede foi repetida nas correções finais. Android final das guardas foi compilado até projeto Gradle; empacotamento direto em andamento para entregar código e binário consistentes.

Classificação deste bloco: aprovado com limitações para cooperação privada LAN Windows–Android. Etapa7 global permanece parcial porque UGS/internet não está configurado/testado, faltam latência/sessão humana completa e ajustes de apresentação. Não avançar a expansão/conteúdo/lançamento como se esses requisitos estivessem prontos. Ver ANDROID_TESTE_FISICO_2026-10-14.md e REDE_PROGRESSAO.md. A etapa de interface completa ainda não foi implementada nesta entrega.

APK final das guardas montado com Gradle BUILD SUCCESSFUL29s, assinatura verificada, instalado-r no moto g15 e iniciado. SHA2569069C045C6B9AF3FFAED818EFABC0D293171C90D9A3CC321594C1DC8EC5A5F78, versão0.6.1/code7/min25/target35/ARM64. Windows final com guardas/background compilou Succeeded/errors0. Cliente final e host normal (sem-netProbe) conectados realmente2/4 em Lobby, sem exceção nos logs próprios examinados. Dados de QA ficaram em diretórios isolados; save offline anterior preservado. Criado atalho adicional Desktop/Jogos/Jogar ForjaDasMares Coop.lnk; atalho local anterior não mudou.

Pacote de entrega será exportado em outputs/ForjaDasMares-Etapa7-LAN com PCZIP/APK/fonteZIP/guias/XMLs, acompanhado de hashes. Não representa fechamento da etapa de internet nem aprovação de lançamento. Nenhum Editor ou build continua em execução; somente host e aplicativo do telefone estão abertos para teste humano, aguardando INICIAR PARTIDA.

### Complemento: teste com atraso e preparação para o usuário

Dois executáveis Windows0.6.1 testados por proxy UDP local com75ms por sentido, jitter±20ms e perda2%. Movimento/ataque remoto, XP12, baú22XP, repetição sem duplicação, derrota11XP e encerramentoClosed confirmados. Nenhuma Exception/Error nos dois logs. Detalhes, limites e reprodução em REDE_LATENCIA_2026-10-14.md; proxy em Tools/QA/udp_latency.py. Sem mudança de C#/cenas/pacotes/builds, sem nova compilação necessária. Saves de QA isolados. Não equivale a ping de internet nem quatro jogadores sob perda.

PC e moto g15 novamente observados em Lobby2/4 na sala normal para teste do usuário. A rodada ampla de bugs/mecânicas/melhorias ficou adiada conforme pedido; nenhuma etapa foi pulada. UGS sem vínculo continua impedindo concluir internet da etapa7. Classificação permanece aprovado com limitações para LAN, parcial para etapa7 global.

### Preparação de internet e publicação — 15/10/2026

CoopInternetSettings e Resources/CoopInternet.asset centralizam ambiente production, região automática e timeout45s. CoopCloud usa SetEnvironmentName, região Relay configurável e validação do ID vinculado; ausência de configuração mantém offline/LAN. Menu Forja das Mares/Internet/Configuração cria/seleciona o asset sem regravar cenas. A primeira compilação encontrou referência ausente à assembly Unity.Services.Core.Environments; adicionada ao asmdef. Segunda compilação e criação do asset concluídas pela Unity.

Rede PlayMode **20/20 aprovada,0 falhas**, XML work/etapa7/internet-network-tests.xml. Inclui17 anteriores e3 novos: configuração pública/defaults, rejeição de Project ID ausente/inválido e tentativa internet sem vínculo preservando offline/sem iniciar transporte. Esses testes não chamam UGS real. Nenhuma build Windows/Android nova foi produzida nesta rodada; executáveis0.6.1 continuam os anteriores e não contêm esta configuração nova.

Unity Cloud foi aberto para o titular e chegou à autenticação/verificação em duas etapas, aguardando ação pessoal. Nenhum projeto/serviço contratado/conta criada/segredo embutido. Internet permanece **pendente**, sem aprovação comercial. INTERNET_E_PUBLICACAO.md documenta vínculo, teste entre redes, limites host/Relay, pontos de migração reais para servidor dedicado e requisitos Play Store consultados (API36/AAB/16KB/assinatura). Servidor dedicado ainda requer extração do papel server-only, identidade validada e persistência backend; não é troca instantânea de endereço. ESTADO_DOS_PEDIDOS.md distingue os dez pedidos detalhados e registra UI/expansão/lançamento/revisão final pendentes.

Criados: CoopInternetSettings.cs, Editor/CoopInternetSetup.cs, Tests/CoopInternetTests.cs, Resources/CoopInternet.asset e metadados; Docs/INTERNET_E_PUBLICACAO.md e ESTADO_DOS_PEDIDOS.md. Alterados: CoopCloud.cs, Networking.asmdef, este progresso. Arena, controles, cenas, dados de combate e pacote de QA preservados. Próxima ação depende do login/vínculo real, seguida de build e teste Relay PC–Android; não declarar todos os pedidos concluídos.

### Unity Cloud real e reconexão no Relay — continuação15/10

Titular concluiu login; projeto separado ForjaDasMares criado e vinculado (Project ID d8fae52c-f0ba-4d51-8e2b-faf465d77c7a). Windows0.6.2 em InternetWindows compilou e dois processos reais entraram em sessão privada MPS/Lobby/Relay, mesmo mapa, movimento e morte por ataque remoto confirmados. Retorno inicial falhou porque Lobby ainda considerava o cliente membro; implementado cache de referência por projeto/ambiente/perfil SDK e ReconnectToSessionAsync, com fallback limitado e remoção do cache após Leave confirmado. Nenhuma credencial no fonte; sem contratação paga.

Windows corrigido recompilou Succeeded/errors0. Regressão21:20 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Reteste real: cliente encerrado abruptamente e reiniciado voltou à vaga1 com140HP/12XP, Playing2/4;59s entre as capturas pré-queda/pós-retorno. Baú concedeu22XP, repetiçãoFalse sem duplicação; derrota controlada sincronizou11XP/13 itens em cada perfil. Relatório REDE_INTERNET_2026-10-15.md distingue QA, serviço real e limites. Não representa sessão humana completa ou redes distintas.

Adaptações: CoopCloud, CoopSmokeProbe (flags explícitas de QA/perfis SDK separados), CoopInstaller (saídas vinculadas separadas0.6.2/code8), CoopInternetTests, ProjectSettings/UnityConnect e documentação. Primeira preparação Android cancelada para incluir a correção; build Android corrigida em andamento. Executáveis LAN0.6.1 anteriores preservados. Etapa permanece em validação PC–Android/Relay; nenhuma etapa posterior marcada pronta.

### Entrega física do Relay0.6.2 — 15/10/2026

Android IL2CPP/ARM64 gerado; Gradle interno falhou novamente no loopback Java NIO. Montagem direta do projeto recém-gerado com ajuste processual concluiu BUILD SUCCESSFUL1min. APK0.6.2/code8/min25/target35 assinado v2, SHA2568DB2A3C83649CF1E8089AB64AA6002774F1C215843F0B09625EE493E8417F481; instalado-r sem limpar dados. Editor encerrou; dois registros SSAO temporariamente retirados por Android restaurados ao fonte canônico. UnityConnectSettings retornou automaticamente ao estado anterior e não tem modificação persistida; o vínculo do runtime é demonstrado pelo Project ID e serviço real.

Windows e moto g15 conectados por código Relay real2/4: toque básico36→10HP, ataques seguintes mataram alvo e concederam12XP; joystick moveu posição real na autoridade. Após force-stop/reabrir, Android retornou ao mesmo host com140HP/12XP preservados em65s entre capturas. Baú administrativo22XP; derrota controlada mostrou11XP,6moedas e uma Lâmina do Cais incomum no aparelho. Saída do host foi reconhecida. Sem Exception/LogError nos registros próprios examinados. Provas e limites em REDE_INTERNET_2026-10-15.md. Usuário não tem dados móveis: não executado teste entre redes distintas.

Código desse bloco já no GitHub8026b42. Sem novos scripts runtime após os testes/builds: apenas documentação/entrega atualizada. README/COOP_COMO_TESTAR/ESTADO_DOS_PEDIDOS atualizados. Aprovado com limitações para protótipo privado Relay Windows–Android em Wi-Fi; não aprovado para Play Store/economia comercial. Interface completa, expansão e revisão de lançamento continuam pendentes. Atalho Jogar ForjaDasMares Online usa perfil InternetCoopProfiles separado; partida LAN antiga emPlaying foi mantida aberta. Nenhum servidor contratado ou migração de host implementada.

Sessões de QA encerradas. Versão InternetWindows normal aberta sem-netProbe, sala criada pelo botão real e Android conectado novamente; Lobby2/4 observado no PC. Atalho Online fica para teste humano, host deve iniciar a partida. Compilações encerradas; Unity não permanece trabalhando em segundo plano. Entrega em outputs/ForjaDasMares-Internet-0.6.2, com WindowsZIP,APK,fonteZIP,guias,XML e hashes; não equivale à build de publicação.

### Correção focada da câmera — 15/10/2026, versão0.6.3

Retomada após cancelamento: ArenaCamera substitui mudanças instantâneas de inclinação por velocidade limitada22°/s, retorno12°/s, teto75°, espera0.35s e extensão suave0.28s. Direção horizontal preservada; colisão recolhe imediatamente para não atravessar geometria. Snap continua somente para início/reset/teleporte. Quatro novos testes cobrem obstrução, retorno, proporções e pausa. Regressão97:96 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Windows compilou Succeeded/errors0; duas instâncias reais em perfis isolados confirmaram movimento, morte por ataque remoto e12XP. Sem exceções nos logs examinados.

Android nativo compilado; empacotamento interno falhou no Java loopback conhecido. Gradle direto concluiu BUILD SUCCESSFUL1min11s; APK0.6.3/code9 assinado, instalado-r e executado no moto g15. Sala local, movimento por toque e renderização observados; sem exceções no log próprio. A velocidade angular é comprovada pelos testes PlayMode; captura física não substitui avaliação em vídeo. Repetição de Relay não feita nesta revisão. Oclusão parcial por paredes e recolhimento imediato continuam como limites explícitos.

Criados CameraObstructionTests.cs/.meta e Docs/CAMERA_CORRECAO_2026-10-15.md. Alterados ArenaCamera.cs, CoopInstaller.cs, ProjectSettings.asset (versão/code), README, COOP_COMO_TESTAR e este progresso. Cenas/pacotes/saves preservados; SSAO canônico restaurado. Saídas InternetWindows-0.6.3 e InternetAndroid-0.6.3; atalho Online atualizado preservando InternetCoopProfiles. Jogos Windows antigos abertos continuam na versão anterior: reabrir o atalho aplica a câmera nova. Ver relatório para evidências e limites; demais etapas continuam com o estado anterior.

### Câmera estável0.6.4 — revisão após observar a partida

Problema residual confirmado visualmente no Windows0.6.3: caixas ainda alteravam ângulo e distância, apesar do limite de velocidade angular. ArenaCamera preserva direção/braço para obstrução comum; novo CameraOcclusion oculta renderers no caminho do herói, preservando colliders/Renderer.enabled e restaurando estado original após liberação/desativação. Recolhimento de emergência apenas quando a lente realmente sobrepõe geometria. Ocultação binária da peça inteira; uma câmera ativa por cliente, sem split-screen. Parâmetros e limites em CAMERA_ESTAVEL_2026-10-15.md.

PlayMode99 casos:98 aprovados,0 falhas,1 ignorado por exigir projeto não vinculado. Seis testes de câmera incluem distância/ângulo constantes, restauração, piso, colisão e pausa. Windows0.6.4 compilou Succeeded/errors0 e menu foi observado após abrir executável novo. Atalho Online atualizado; saves preservados. Teste humano de enquadramento disponível. Android0.6.4 em preparação; não declarar aparelho atualizado neste checkpoint.

Criados CameraOcclusion.cs/.meta e relatório CAMERA_ESTAVEL_2026-10-15.md. Alterados ArenaCamera.cs, CameraObstructionTests.cs, CoopInstaller.cs e documentação; versão de ProjectSettings acompanha build. Cenas, rede e combate não alterados. Revisão completa do jogo continua com as pendências anteriormente documentadas.

### Semitransparência solicitada pelo titular —0.6.5

Usuário aprovou o enquadramento estável de0.6.4 e pediu manter os obstáculos parcialmente visíveis. ArenaCamera expõe Obstruction Opacity padrão35%. CameraOcclusion usa materiais transparentes temporários/cache em vez de desligar desenho; mantém colliders e flags de visibilidade, restaura arrays originais e destrói cópias ao desativar/resetar. Template real Resources/CameraOcclusionTransparent.mat conserva variante URP/Lit transparente na build. Nenhuma alteração de combate/rede/cenas/saves.

Dois passes específicos7/7 aprovados,0 falhas, último com assert de material do chão. Não repetida suíte ampla anterior99 casos depois deste ajuste. Windows0.6.5 compilado Succeeded/errors0, atalho Online atualizado e menu aberto observado. Aviso de Firewall exige escolha pessoal do titular; QA visual de gameplay não foi declarada concluída neste checkpoint. Android0.6.4 cancelado para incluir pedido; Android0.6.5 em compilação. Telefone não aparece mais no ADB, portanto teste físico novo está pendente.

Arquivos: ArenaCamera.cs, CameraOcclusion.cs, CameraObstructionTests.cs, CoopInstaller.cs e documentação; novos Resources/CameraOcclusionTransparent.mat/.meta e CAMERA_TRANSPARENCIA_2026-10-15.md. Limitações de alpha blending/peça inteira/uma câmera e shaders futuros estão no relatório.


Fechamento0.6.5: fonte do efeito no GitHub72c1253. APK Android0.6.5/code11 real produzido após compilação nativa e Gradle direto BUILD SUCCESSFUL1min9s; assinatura v2 e badging verificados. SHA2569233B565C662B577276D879193CB033512B0358DB7C3CAAA0E46C1AFE5C1ADAA. Salvo em Builds/InternetAndroid-0.6.5, sem instalação/teste físico porque ADB não encontra dispositivo. PC pronto/aberto, aviso Firewall exige ação pessoal para jogar; aparência final ainda depende de avaliação humana. Sem teste Relay novo. URP SSAO canônico restaurado e compiladores encerrados. Classificação aprovada com limitações para o ajuste de semitransparência. Outras etapas do projeto conservam o estado anterior.

### Galerias da Maré — primeiro mapa original de estilo,0.6.6

Variante local jogável salva com piso modular chanfrado, alvenaria segmentada, grades, caixas/baús de madeira e lanternas geométricas com luz real;9 salas em rota original, usando combate/progressão anteriores.11 meshes originais salvos,8 materiais e dados de salas/bioma/estilo. DungeonStyleDefinition opcional conserva cenas antigas; gen/rota/links de progressão receberam extensões pequenas. GaleriasDungeon e GaleriasArena separadas; perfil e atalho próprios. Online0.6.5 e arena/referências preservados.

Regressão inicial105:99 da base aprovados,5 novos falharam por refs não gravadas,1 ignorado. Corrigido instalador/recarregamento/validação de refs; rodada final5/5 dos testes do mapa aprovada após corrigir expectativas de teste sobre portas fechadas e nomes de assets. Não afirmar que XML inicial está todo verde. Fluxo completo com dano de QA para matar inimigos/boss, objetivos, baú, chave, armadilha, resultado/deduplicação e retorno da arena passou. Não é vitória humana.

Windows0.6.6 compilou Succeeded/errors0 e abriu. Binário real passou smoke write/read com perfil isolado:13itens,5XP,5moedas, armadura/max160 e opções persistidas. Captura mostra novo cenário renderizado, com aviso Firewall cobrindo centro e exigindo escolha pessoal. Sem erros nos logs próprios examinados. APK em compilação; não instalado/testado em telefone desconectado. Destruição, arte final, profiling sustentado e integração deste mapa no cooperativo continuam fora deste recorte. Ver MAPA_GALERIAS_2026-10-15.md para arquivos/procedimento/limites.


Fechamento Galerias0.6.6: APK separado com.forjadasmares.galerias/code12/ARM64 real produzido; Gradle direto BUILD SUCCESSFUL48s após Java loopback interno. Assinatura v2 e badging verificados; hash E55F102991AA944A827263575BDF0906D5FB6C643D2ECE132CAA7E9F5A05D500. Não instalado nem testado fisicamente porque ADB está sem aparelho. Fonte já no GitHubd7b32f3; documentação/versão/finalização em novo checkpoint. Restabelecidos productName/ID principais e SSAO canônico; builder passou a salvar a restauração de identidade, recompilou e validou novamente referências de8 salas/bioma/estilo. Nenhum processo Unity/build ativo ao encerrar. Aprovado com limitações para mapa local original/provisório; não representa90% do jogo de referência nem integração nova no online. Lista de arquivos em MAPA_GALERIAS_ARQUIVOS.md.

### Galerias0.6.6 — aparelho reconectado e verificado

Moto g15 autorizado reconectado: instalado APK separado com.forjadasmares.galerias0.6.6/code12 e executado fisicamente. Seed73129, mesmos assets de piso/alvenaria/lanternas/caixas/grades, joystick levando da entrada ao combate, ataque/habilidade e conclusão real da primeira sala (+39XP/8 moedas temporários), dano ao jogador, desbloqueio de grades, esquiva isolada e parede semitransparente com colisão/restauração observados. Sem exceções Unity ou falhas fatais no log restrito ao processo examinado. Android2400×1080 mostra mais nas laterais que PC1280×720; HUD ainda volumoso/com atalhos de PC. Aprovado com limitações para esta verificação local; sem profiling sustentado, vitória completa ou novo teste online. Nenhum código alterado ou recompilado. Relatório GALERIAS_ANDROID_2026-10-15.md e adendo ao relatório do mapa; evidências locais em work/galerias/android-*.png e android-device.log. Aplicativo online e dados preservados.

### Câmera contínua — checkpoint antes do HUD

Dois defeitos relatados reproduzidos por testes: salto do pivô virtual em caixa2.153m e mudança de foco7m entre salas. Removido reset da mola no pivô; DungeonSession acompanha livremente o jogador e mantém cenário desenhável entre encontros. Sem alteração de combate/recompensas/geometria. Regressão107:106 aprovados,0 falhas,1 ignorado por vínculo Cloud. Avisos nativos JobTempAlloc registrados para investigação separada. Arquivos ArenaCamera.cs, DungeonSession.cs, GaleriasInstaller.cs, CameraObstructionTests.cs, GaleriasMapTests.cs e CAMERA_CONTINUA_2026-10-15.md. Builds0.6.7 serão feitas após HUD pedido durante o trabalho; nenhum binário novo foi declarado pronto neste checkpoint.

### HUD rústico — checkpoint PC0.6.7

Novos RoundedHudGraphic/HudIcon/RusticHudTheme/RusticDungeonHud e teste RusticHudTests com metas; DungeonHud e ProgressionHud integrados à apresentação compacta, ícones originais e painéis arredondados. Vida/XP/ouro/objetivo reais, mochila/menu, controles existentes reaproveitados, cooldowns, toque em safe area, ferramentas de teste recolhidas. Tema verde/cobre/pergaminho provisório. Sem assets externos/licenças adicionados. Primeira rodada identificou Graphic duplicado; resolvido por superfície filha. Última17/17 aprovada,0 falhas. Windows0.6.7 compilado e aberto; Firewall exige ação pessoal e limita revisão visual. Android compilando. HUD local de masmorras PC/Android; lobby online continua apresentação própria. Não declarar teste físico novo ou profiling. Relatório HUD_RUSTICO_2026-10-16.md.

### Refinamento do HUD0.6.8 — testes e Windows

QA físico0.6.7 no moto g15 confirmou renderização, menu/inventário, seleção/comparação de item, fechamento, joystick, semitransparência e acionamento da habilidade com cooldown. Identificados e corrigidos trilho XP sem preenchimento vertical e controles aparecendo sob menu. Última rodada17/17 passou, novo assert de altura positiva. Windows0.6.8 real compilado Succeeded/errors0; atalho atualizado preservando argumentos/perfil. Android0.6.8/code14 em preparação. Nenhum novo teste online ou medição sustentada. Câmera/UI fonte protegida no GitHub; relatório HUD_RUSTICO_2026-10-16.md.

### Fechamento câmera e HUD local0.6.8 —16/10/2026

Windows0.6.8 compilado Succeeded/errors0, atalho atualizado; smoke real write/read com13 itens/5XP/5moedas/armadura/max160/opções, saídas0. Última suíte específica17/17 aprovada (8câmera/6mapa/3HUD); suíte ampla107 antes do HUD106 aprovados/0falhas/1ignorado. Não confluir essas rodadas em teste amplo final inexistente. Avisos nativos JobTempAlloc em batch registrados como risco separado.

APK0.6.8/code14/ARM64 real50.804.539 bytes, assinatura v2 verificada, SHA256 F8D4AEC43D00F5EEA818DCE1021472EA502C0B6A3DAE6B82C18CCD7F6FE95695. Java loopback interno falhou; Gradle direto BUILD SUCCESSFUL45s. Instalado-r no moto g15 e executado. Capturas finais confirmam XP preenchido39/60, menu sem controles sobpostos, inventário por toque, retomada do joystick e habilidade/efeito/cooldown5s. Sem exceções Unity/falhas fatais nos logs próprios examinados. Sem partida completa, gamepad físico, profiling sustentado ou teste online novo. PC visual limitado por aviso Firewall que deve ser respondido pessoalmente. Aprovado com limitações para este recorte, não jogo completo.

Novos4 componentes visuais e1 teste com metas; alterados2 HUDs existentes, ArenaCamera/DungeonSession,2 testes prévios, builderGalerias, versão/code e documentação README/MAPA/CAMERA_CONTINUA/HUD_RUSTICO/PROGRESSO. Cenas/arte de mapa/pacotes/saves preservados; SSAO restaurado, identidade principal restaurada. Fonte em checkpoints GitHub3d57040/a115647/7e51b38 e finalização documental posterior. Relatório inclui arquivos, passos manuais e limites. Usuário pediu descanso da tela após concluir; nenhuma suspensão durante compilações.

### Brasa3D — Blender e integração inicial

Blender4.5.14 LTS portátil instalado em Area Dev/Ferramentas, hash oficial do ZIP validado. Atalhos Blender e Editar personagem Brasa. Criado personagem autoral no Blender com corpo/rosto/cabelo/roupas/armadura/espada, anatomia unificada,17 ossos/8 clipes reais,7.898vértices/15.262triângulos/13materiais, render Cycles e fonte .blend/.py. FBX/prefab/controller/materiais integrados somente nas cenas Galerias, preservando referências antigas desativadas. Outros heróis e online não recebem modelo automaticamente.

Importador passou2vezes com8clipes/2cenas. Suíte114:112aprovados/1falha de encontro4 no fluxo cooperativo/1ignorado Cloud. Quatro testes novos de modelo passaram. Corrigida captura de pose também em Configure de HeroEquipment criado em runtime; repetição focada pendente. Não marcar todos os testes ou builds concluídos. HeroModelView opcional, HeroPresentation e equipamento adaptados sem mudar dano/atributos. Arquivos e limites em BRASA_MODELO_2026-10-16.md. Preview/modelo são primeira versão autoral, sem arte/animação final nem profiling móvel.

### Brasa3D — fechamento0.7.0

Repetição focada26/26 aprovada, incluindo o percurso cooperativo que falhara; suíte inteira114 não repetida. Windows0.7.0 compilado Succeeded/errors0, aberto/inspecionado com novo modelo e acionamento da habilidade; atalho atualizado. Save smoke write/read real passou em perfil isolado,13 itens/5XP/5moedas/armadura/max160/opções. Blender aberto e fonte enquadrada/salva.

APK0.7.0/code15/ARM64 real51.601.758bytes, assinatura v2 válida, SHA256 F74D9882C9FC4140B361C156134FF4F3AA1C4C9732147D4D2DCF88827E026FDD. Empacotador interno falhou no Java loopback; Gradle direto concluiu em1m5s. Instalado-r e executado no moto g15; modelo/material, joystick/câmera, habilidade/efeito/cooldown e esquiva/deslocamento/cooldown observados. Ataque acionado sem comprovação visual de pose intermediária ou dano nesse teste físico. Sem exceções Unity/fatal/Animator nos logs próprios examinados.

Fonte .blend/.py, FBX/prefab/controller/13materiais, HeroModelView, installer/testes e integração em GaleriasArena/GaleriasDungeon protegidos no checkpoint GitHub389fa44. Finalização altera README, relatório, progresso e versão/code; identidade principal e URP preservadas. Primeiro personagem autoral funcional, não arte/animação final; outros heróis, online e demais armas mantêm visuais anteriores. Sem profiling sustentado nem teste de rede novo. Relatório BRASA_MODELO_2026-10-16.md contém arquivos, configuração, evidências e limitações.

### Brasa0.7.1 — correção de sobreposição e novos ataques, checkpoint Windows

Confirmadas cópias do mesmo prefab sobrepostas: instalador procurava Brasa_Model, mas SaveAsPrefabAsset nomeava a raiz Brasa. Limpeza seletiva por HeroModelView/caminho do prefab e identificação independente de nome; reexecuções preservam1 instância por cena. Defesa de apresentação desativa cópias antigas e mantém placeholders referenciados desativados. Controlador de animação passou a ter um dono, sem Animator.Play a cada quadro/AnyState concorrente. Pesos anatômicos do peito corrigidos, apoio dos pés refeito e12 clipes reais com3 golpes distintos/pesado/super.

Ataque segurado repete combos. Nas Galerias,1,35s carrega pesado2,4x;16 acertos em inimigos enchem Ruptura de Brasa, Q/botão SUPER. Buffer permite solicitar durante outro golpe; não empilha pedidos. Dados configuráveis, dano/parede/deduplicação reaproveitados,10 fitas reutilizadas sem colliders. Arena também tem barra/botão funcional. Mantidos4 heróis e dados de progressão; modelo/poderes novos locais, não integração online concluída.

Rodada ampla final123 casos:122 aprovados/0falhas/1ignorado Cloud. Depois do buffer:42 focados com41 aprovados/1falha do percurso cooperativo já intermitente; robô ignorava obstáculos. Corrigido só o caminho do robô no teste, preservando asserções/dano/colisões/recompensas;42/42 passaram com caminhos bloqueados registrados nas salas1/2/4. Refinamento de efeitos para projéteis:31/31 passaram. Não confluir essas rodadas em uma suíte ampla posterior inexistente.

Windows0.7.1 compilado Succeeded/errors0; smoke real write/read aprovado em perfil isolado (13 itens/5XP/5moedas/armadura/max160/opções). Atalho atualizado preservando perfil original. Janela nova de QA aberta; observados combate humano, carregamento56%, super50% e depois88%, avanço de salas/saque e apenas1 corpo visível. Sem exceções de referência/Animator nos logs próprios examinados. Android em compilação neste checkpoint. Fontes, dados, listas de arquivos, testes e limites em BRASA_COMBATE_2026-10-16.md. Avisos nativos JobTempAlloc em batch e revisão artística/profiling seguem pendentes.


### Brasa0.7.1 — fechamento Android do checkpoint anterior

APK real 51.726.226 bytes, SHA256 57A4559F45C87FEE29C9028B2CED38AB6567056FE60578C0F44ACE99AA62DC1B, ARM64/code16, assinatura v2. Gradle direto concluiu após falha local de loopback Java no empacotador interno. Instalado-r e executado no moto g15; toque mantido mostrou carga64%, joystick/habilidade acionados e vídeo próprio android-brasa-combat.mp4 gravado. Super não foi ativado no teste físico. Fonte desse bloco protegida no GitHub e00c41f. Usuário identificou locomoção ainda rígida/deslizante; qualidade de animação reaberta para0.7.2.

### Brasa0.7.2 — locomoção, em validação

Auditados clipe curto e substituição das pernas pelos ataques. Nova corrida, ciclo por distância real, ações superiores mascaradas, apoio dos pés com IK de dois segmentos e postura parada com respiração. Primeiro teste detectou desvio0,06554; corrigido alcance do quadril sem relaxar limite0,06. Rodada seguinte32/32 aprovada. Últimos refinamentos e builds ainda em verificação; não considerar encerrado. Arquivos, configuração e limites em BRASA_MOVIMENTO_2026-10-16.md.


Checkpoint Windows0.7.2: rodada final32/32 aprovada, incluindo corrida/lateral rápido e regressão de golpes; compilação Windows Succeeded/errors0. Atalho atualizado e build aberta no perfil original (nível5/53XP/112ouro observados). Postura/entrada de movimento conferidas; sem erros de referência/Animator no log examinado. Android em compilação; aparelho bloqueado, teste físico depende de desbloqueio pessoal. Não há medição sustentada de FPS nem teste online novo.


Fechamento0.7.2: fonte e arte protegidas no GitHub2f2d31e. APK real51.782.174bytes, SHA256 0B0DAD351222CA661E3A044E0DF9307CD176C9786F3A5DED8D68FD4E7B075C77, ARM64/code17/assinaturav2. Empacotador interno Android falhou por conexão loopback Java/errors4 após compilação nativa; Gradle direto concluiu BUILD SUCCESSFUL in57s/exit0. Instalado-r Success no moto g15; versão instalada0.7.2/code17 confirmada. Celular ainda bloqueado (keyguard showing=true), portanto não houve teste visual dessa revisão nem medição de FPS no aparelho. PC continua aberto, atalho aponta0.7.2,32 testes focados passaram; qualidade artística final e teste prolongado permanecem em revisão. Relatório BRASA_MOVIMENTO_2026-10-16.md lista arquivos/procedimentos/limitações. Identidade principal e URP preservadas; versão/code atualizados.


### Campanha0.8.0 — 17/10/2026, implementação em validação

Retomado após pausa explícita do usuário. Auditoria somente antes da pausa; nesta retomada autorizada, criada variante CampaignCoop com3 mapas12/15/18 setores, extensões opcionais do gerador, placas de2 heróis, flechas autoritativas e névoa exterior. UI com mapa de fases, sala privada/código e procura de grupo público por fase. Catálogo separado com inventário inicial vazio, prévia3D e5 slots; saves anteriores preservados. Protocolo3/versões antigas incompatíveis intencionalmente.

Instalador compilou e criou assets reais. Primeira rodada19/22, três falhas de MaterialPropertyBlock no construtor das placas; corrigido. Segunda23/23 aprovada. Suíte ampla, builds e teste real de descoberta/PC–Android ainda pendentes neste checkpoint. Não marcar campanha concluída. Detalhes/limites/arquivos em CAMPANHA_2026-10-17.md. Android0.7.2 anterior gravado com toque no aparelho desbloqueado; não confundir com teste0.8.0.

### Campanha0.8.0 — fechamento do candidato de teste

Checkpoints publicados1bfa045 e be1f262. Regressão ampla138 casos/137 aprovados/1 ignorado/0 falhas; refinamentos depois39/39 e13/13 focados. Duas builds Windows concluídas, final Succeeded/errors0. Dois processos Windows pelo Relay descobriram sala pública da fase e testaram movimento/ataque/XP, resgate, baú sem duplicação, desconexão/reconexão e fechamento do host. Conclusão administrativa testou resultado/persistência e continuidade12→15→18 setores mantendo o grupo/código; não equivale a vitória humana completa. Relógio de expedição e nova confirmação de carregamento por tentativa; entregas recuperáveis e resultado rolável.

Android interno falhou com erro loopback Java; Gradle direto do projeto gerado concluiu BUILD SUCCESSFUL/exit0 em1min3s. APK real51.840.222bytes, SHA2564130EDE9F6CC8EF8CEF406F2EAD99ED90DF8506B18EC24082A2F749463667F0D, com.forjadasmares.campaign0.8.0/code18/ARM64/API25–35/assinatura v2 verificada. Celular ausente em adb devices: APK não instalado nem campanha física testada nesta rodada. Identidade principal Arena/prototype e URP preservadas; versão/code atualizados. Atalho Jogar Campanha Forja das Mares criado em Desktop/Jogos, sem argumentos de QA. Builds continuam fora do Git.

Arquivos desta etapa e limitações em CAMPANHA_2026-10-17.md; instruções práticas em CAMPANHA_COMO_JOGAR.md e README. Documentação lista novos dados/cena/shader/testes, extensões de gerador/interações, rede/atores/progressão, apresentação e inventário. Pendentes: ação pessoal no Firewall, teste campanha0.8.0 PC–Android, prévia3D/UI em GPU, vitórias humanas cronometradas, quatro clientes/redes diferentes. Sem migração de host, conta global ou pesado/SUPER online. Status aprovado com limitações para teste Windows em dupla; candidato Android ainda sem validação física. Próximo trabalho deve priorizar essas validações e ajustes observados, sem afirmar conclusão comercial.
### Campanha0.8.1 — solo, salas abertas e interface inicial

Pedido retomado: todos os mapas podem iniciar com1–4, solo ×1,10 vida/dano temporário, placas sequenciais no solo, abrir/privatizar a sala e achar partidas da mesma fase em andamento antes de20%. Tela inicial funcional com campanha, agenda local de convites, volume e saída confirmada. Não foi implementada conta social global de amigos. Preservados cenas/saves/catálogos antigos; campanha continua em CampaignProfiles-v1.

Auditoria/testes acharam e corrigiram: referência nula de progresso antes da geração; consulta do HUD no objeto errado; perfil inicial enviado antes do handler do cliente, bloqueando mochila até recompensa; tentativa de usar nova vaga/abrir outro escritor do mesmo save após expirar90s. Corrigidos com progresso0 antes do mapa, referência ao Canvas real, solicitação/reenvio inicial limitado e reserva vinculada ao ledger atual + proteção contra identidade duplicada. Fórmula do bônus centralizada e usada na comparação de itens, sem cura por alternar solo/grupo. Chegada é posicionada no setor vigente depois de confirmar o mapa.

Regressão final de rede34 casos:33 aprovados,0 falhas,1 ignorado por projeto UGS já vinculado. Builds Windows finais Succeeded/errors0. Android exportou nativo, mas empacotador interno teve erro loopback Java/errors4; Gradle clean direto terminou BUILD SUCCESSFUL/exit0 em1min7s. Uma inicialização do Editor falhou nativamente durante os ciclos de plataforma; repetição com Burst síncrono concluiu; causa não atribuída definitivamente. APK final51.858.010bytes, SHA256EF982BD9E499BBBE745FF15FBABEB380CE6DB123E856F4BBC288B911A8A15E06,0.8.1/code19/com.forjadasmares.campaign/ARM64/API25–35/v2 verificado, instalado-r Success no moto g15. Atalho PC aponta CampaignWindows-0.8.1. Identidade principal Arena/prototype e referências URP preservadas; bundleVersion/code atualizados.

Testes reais: Windows começou solo154HP, abriu sala e recebeu cliente em Playing, voltou140HP; terceiro bloqueado depois de27,27% administrativamente concluídos; participante reconectou após corte. Android físico navegou na tela inicial, salvou convite/preservou-o ao atualizar, entrou pelo convite em fase iniciada, moveu/esquivou e mostrou prévia3D. Após correção, mochila abriu no lobby novo antes de XP/itens. Na revisão final, Android encontrou host público, jogou, saiu, ficou desconectado por mais de148s e voltou à mesma vaga1, ambos140HP. Hashes dos dois perfis não mudaram no retorno; sem expedição/perfil duplicado. Host encerrou Closed/0 conectados, sem erro de remoção nesse teste. Logs finais examinados sem NullReferenceException/MissingReferenceException/InvalidOperationException. Salas/processos de QA encerrados; aplicativos normais reabertos sem flags de teste.

Arquivos: novo CampaignHome; alterações CampaignHud/Run/Plates, CoopDirector/Cloud/Progression/RewardLedger/ProfileChannel/Protocol/SmokeProbe/InventoryHud, HeroEquipment/ProgressionStats, CampaignInstaller/Tests e descrição Level1. Documentados em CAMPANHA_SOLO_2026-10-17.md, CAMPANHA_COMO_JOGAR.md, README e ARQUITETURA. Checkpoints b05a756/c21ca70/0f69621/93e335d publicados no GitHub; builds/evidências privadas continuam fora do Git.

Status aprovado com limitações para testes solo e PC–Android em dupla no Wi-Fi. Pendentes: quatro dispositivos, redes distintas, vitória humana cronometrada nas três fases, balanceamento/FPS prolongado e amigos com contas. Sem migração de host, save global, pesado/SUPER online ou aprovação Play Store. Não equivale a concluir todos os pedidos históricos nem a entregar um lançamento comercial. Próximo trabalho deve partir dos problemas observados nas partidas humanas.

## HUD e compartilhamento 0.8.2 — 17/10/2026
HUD compacto com código ao vivo; menu local de sala/grupo/convites/configurações/mochila/saída confirmada. Builds PC/Android produzidas, 34 testes passaram/1 ignorado/0 falhas. PC host + moto g15 cliente testados no Wi-Fi; timeout na tentativa com Android host registrado. Arquivos, evidências, hashes e limitações em HUD_E_COMPARTILHAMENTO_2026-10-17.md.

## Paisagens, hordas e inventário0.10.1 — 17/10/2026

Campanha ampliada para setores28/30/32m, passagens18m, sem portas fechando. Encontro preservado durante transição e conclusão creditada ao setor correto; saída exige objetivos obrigatórios. Hordas difíceis109/136/164inimigos acumulados, cap conforme host20Android/24PC. Três elites e modelos originais por arquétipo. Ícones por categoria, comparação, SAQUE ATUAL e equipagem durante fase fora de perigo; conclusão com mochila cheia preserva item usado. Combos de três ataques nas armas, carregado/SUPER nos quatro heróis, suprimentos curam uma vez, feedback por áudio/números.17FBX/12PNG/4WAV e galeriaBlender originais; não são arte/clips finais.

Correções encontradas em QA: arma herdava escala dos ossos e cobria câmera — compensação compartilhada host/réplica/troca de visual; pontos de spawn antigos restaurados; câmera torna filhos de props parcialmente transparentes; pool recupera componentes após estado de réplica; mensagens técnicas de recompensas removidas; migração de combos idempotente.

160testes PlayMode:159aprovados/0falhas/1ignorado;16focados aprovados. Windows0.10.1 BuildPipeline errors0. Android0.10.1/code24 ARM64 gerado via Gradle externo depois de erro de loopback no empacotador interno; assinatura/hash verificados e atualização instalada no moto g15. PC+Android Relay no Wi-Fi: entrada, equipagem, movimento, combate/derrota, limpeza e encerramento. Teste4/4 da build intermediária0.10.0:3processosWindows+1Android. Não quatro humanos/dispositivos; não vitória humana, redes diferentes ou FPS medido.

Relatório/arquivos/passos manuais: PAISAGENS_E_HORDAS_0_10_2026-10-17.md, ARQUIVOS_0_10.txt, CAMPANHA_COMO_JOGAR.md e ASSETS_E_DIRECAO_0_10.md. AtalhoPC e APK compartilhável atualizados para0.10.1, salasQA encerradas e saves preservados. Aprovado com limitações para testes privados; não equivale a concluir todos os pedidos históricos ou lançamento comercial. Importantes: performance/balanceamento humano, mapas ainda modulares/encontros serializados e arte/animação final. Nenhuma compra necessária agora.
