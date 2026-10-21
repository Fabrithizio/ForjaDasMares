# Revisão 0.14.2 — HUD sem botões fixos de marcação

- Removidos os quatro botões AQUI/ALVO/PERIGO/SAQUE e seu painel do HUD, tanto no PC quanto no celular. Nenhum objeto dessa barra fica invisível interceptando cliques ou toques.
- Marcação AQUI por G/direcional para cima continua opcional; o protocolo e o sistema de rede foram preservados.
- Testes antigos que exigiam a barra foram substituídos por uma regressão que entra na partida e confirma a ausência dos objetos, incluindo inativos. Grupo de aventura/interface: **24/24 aprovados**, zero falhas/ignorados.
- Builds Windows e Android: Succeeded errors=0, encerramento batch código0. APK real0.14.2/código30, ARM64, mesmo pacote com.forjadasmares.campaign. Protocolo10/savev2 preservados.
- PC conferido na build jogável; APK Android produzido. Teste físico desta pequena revisão pendente.
- Saída única atual: Builds/Atual/Android/ForjaDasMaresCampanha.apk. Enviar esse arquivo como Documento no WhatsApp; não usar as pastas históricas de envio. PC atualizado em Builds/Atual/Windows/ForjaDasMaresCampanha.exe. Preferir ambos os aparelhos na0.14.2.
- Arquivos alterados: CampaignAdventureHud.cs, AdventurePolishTests.cs, CampaignInstaller.cs, ProjectSettings.asset e documentação. Nenhuma cena, save ou pacote substituído.
- Evidências e hash do APK em Evidencias_0_14_2/validacao.json.

Limitações anteriores mantidas: host sem migração, progresso por host, arte provisória e validação prolongada/humana pendente. Não repetida a suíte geral nesta correção de interface; o resultado geral anterior permanece no relatório0.14.1.
