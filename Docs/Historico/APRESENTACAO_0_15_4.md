# 0.15.4 — início, campanha, entrada e nitidez

## Ordem aplicada
1. Início: arte original, hierarquia Jogar / Personagem / Convites / Configurações, versão discreta.
2. Campanha: mapa ilustrado, três fases reais selecionáveis, dificuldade e condição de conclusão visíveis; indicação explícita de que todas estão disponíveis no teste.
3. Entrada: Jogar solo cria sala privada e inicia após conexão; criar privada mantém lobby para convidar; procurar grupo reutiliza busca existente e cria aberta se não houver vagas; entrada por código tem validação local.
4. Lobby: quatro espaços de jogadores, heróis reais, seleção por cartões em duas colunas para os quatro heróis atuais, com dados dos ScriptableObjects, código copiável, início restrito ao host, aviso do encerramento quando ele sai.
5. Estado: carregamento/conexão com cancelamento, sem percentuais fictícios; mensagens úteis de falha, retorno ao início com motivo real. Quando o host encerra a sala, o aviso pede novo código, sem prometer retorno à sala encerrada. Sem migração de host inventada.

## Serrilhado
Causas verificadas nos arquivos: RoundedHudGraphic/HudIcon sem cobertura alfa nas bordas; PC_Renderer em Deferred com MSAA desligado; Mobile_RPAsset com MSAA1 e renderScale0.8. Overlay Canvas não recebe o antialiasing da câmera da cena.

- Bordas arredondadas: 64 segmentos e faixa de transparência de um pixel físico. Ícones de linhas recebem borda equivalente.
- PC: mantém Deferred; SMAA Medium no Equilibrado, High no Nítido; Econômico usa FXAA e renderScale0.8.
- Android: mantém Forward; Equilibrado renderScale1/MSAA2, Nítido1/MSAA4, Econômico0.8/MSAA2.
- PresentationQuality usa cópia runtime do pipeline, restaura e destrói na saída; não modifica os assets de renderização ao trocar opção.
- Prévia do personagem: resolução maior e SMAA. Não foi feito remodelamento de personagens ou cenário.
- Opção em Configurações do início e do menu da partida. Sem prometer FPS universal ou ausência total de aliasing, especialmente em efeitos transparentes e escala reduzida.

Fundamentação: implementação URP17.3 instalada, UniversalAdditionalCameraData e UniversalRenderer; [comparação oficial de recursos](https://docs.unity.com/en-us/engine/6000.3/manual/render-pipelines/choose-a-render-pipeline/feature-comparison). Sem novos pacotes externos. Runtime passa a referenciar as assemblies URP/Core já instaladas.

## Como testar
- Início → Jogar: escolha uma das três fases.
- Herói: clique no cartão da classe, sem precisar percorrer todas em um botão.
- Solo: aguarde criação pela internet e carregamento automático. Não há bônus solo.
- Criar sala privada: copie o código no lobby; outro aparelho usa Entrar pelo código. Ambos precisam da mesma versão.
- Procurar grupo: seleciona a mesma fase; se não houver grupo, cria aberta. Inicie pelo botão do host.
- Durante carregamento, Cancelar e voltar deve encerrar a tentativa com segurança.
- Em partida, Esc/Menu → Sala e grupo mostra código e convites. O host sair ainda encerra a sessão; a interface informa isso.
- Configurações → Gráficos alterna Equilibrado → Nítido → Econômico. Verifique bordas e desempenho no seu aparelho.

## Arquivos
Alterados: CampaignHome.cs, CampaignHud.cs, CampaignSessionMenu.cs, CoopCloud.cs, CoopDirector.cs, CoopHud.cs, Editor/CampaignInstaller.cs (Networking); HeroRoster.cs; EquipmentPaperDoll.cs; HudIcon.cs; RoundedHudGraphic.cs; ForjaDasMares.Runtime.asmdef; Assets/Settings/Mobile_RPAsset.asset; versão em ProjectSettings.
Criados: Networking/CampaignHeroPicker.cs; Networking/Editor/MenuArtImporter.cs; Networking/Tests/CampaignPresentationTests.cs; Scripts/UI/MenuArtwork.cs; Scripts/UI/PresentationQuality.cs; duas artes em Resources/MenuArt. Metadados Unity correspondentes.

## Verificação e entrega
99/99 testes finais PlayMode aprovados, Unity exit0 (19 casos novos de apresentação; 80 de regressão). Builds finais Windows e Android Succeeded/errors0/exit0. APK0.15.4/código35/API25–35/ARM64 produzido no caminho habitual.

Conferência nativa Windows1280×720: início, campanha, seleção dos quatro heróis, solo automático, sala privada, lobby e início pelo host. Duas instâncias Windows reais pelo Relay: mesmo mapa e heróis, saída de cliente sem fechar o host, retorno à vaga, host encerra e cliente volta ao início com aviso em português pedindo novo código. A última repetição usa a build final. Busca de grupo via sonda de desenvolvimento em índice isolado criou um lobby público na ausência de vagas. Não foi teste com duas pessoas.

**Complemento em 24/10: moto g15 reconectado e atualizado por install -r, Success; versão0.15.4/código35 conferida no pacote.** Mesma aplicação, sem desinstalar ou apagar dados. Conferidos início, campanha e seleção de heróis em2400×1080; navegação por toque. PC e Android reais entraram pelo Relay, receberam o mesmo mapa e heróis (Guerreiro/Arcanista), iniciaram a fase e replicaram o movimento do joystick (aprox.1,43m observado pelo host). Escolha de altar por toque e encerramento do host também conferidos. Saves de QA isolados; aplicação reaberta sem flags de teste ao final. Não foi campanha completa, teste com quatro jogadores, controle manual simultâneo por duas pessoas ou teste deliberado em redes diferentes. Evidências e hash em Evidencias_0_15_4/entrega.json.

Na primeira tentativa física, o celular entrou no lobby e fechou a sala local antes do início; causa não confirmada, sem exceção capturada. A repetição completou o fluxo descrito acima. Monitorar se isso voltar a acontecer sem interação. Sombras móveis de baixa resolução ainda mostram degraus; antialiasing de geometria/UI não elimina esse aspecto. Falta medir FPS em combate pesado no aparelho. Testes usam saves isolados; não usar o perfil normal do jogador como fixture.

## Limites preservados
Convites/amigos continuam agenda local de códigos, sem presença online. Não foi criado backend de conta global ou migração de host. Perfis permanecem locais/guardados pelo host, savev2, protocolo10. Artes são ilustrações para as telas, não mapas/modelos 3D novos. Balanceamento, loot e ataques não foram alterados nesta entrega. As combinações e telas não podem ser consideradas universalmente validadas por uma única resolução ou aparelho.

[Artes e prompts completos](ARTES_MENU_0_15_4.md).
