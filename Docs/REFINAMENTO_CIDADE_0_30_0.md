# Refinamento visual e organização — 0.30.0

Pedido: reduzir o aspecto geométrico/chapado do kit, continuar integração e manter os materiais organizados com orçamento móvel.

## O que mudou
- Revisão ativa do kit em ArtSource/CityLifeKit/Refinado: materiais de madeira, pedra, tecido e metal com detalhes procedurais em atlas 768x512; UVs de superfície e normais por ângulo, sem subdivisão adicional.
- Três modelos novos: placas com símbolos de padaria, ervas e oficina. Total atual 18 modelos / 36 exportações LOD; não são 36 objetos distintos de conteúdo.
- Prefabs e GUIDs anteriores preservados, usando o atlas refinado. Um material/submesh por LOD continua sendo a regra. Mipmaps, compressão e gutters reduzem serrilhado e mistura de tiles, mas precisam de conferência móvel.
- Cidade recebe praça e caminhos pavimentados sobre áreas verdes, em uma malha visual combinada. A física do chão e a navegação existentes são mantidas. Sem sombras dinâmicas adicionais ou novos scripts por barraca.
- Seta, saída terrestre, acesso às expedições, NPCs provisórios e reservas de eventos/expansão preservados.
- Índice ArtSource/LEIA-ME.md identifica fonte ativa, base recuperável, previews, catálogo e arquivos Unity. Não foram movidos assets referenciados, apagados modelos antigos ou reorganizadas pastas pessoais fora do projeto.

## Arquivos principais
- ArtSource/CityLifeKit/Ferramentas/refinar_cidade_viva.py e validar_refinado.py.
- ArtSource/CityLifeKit/Refinado/* e ArtSource/LEIA-ME.md.
- Assets/ForjaDasMares/Art/World/CityLifeKit/*: FBX existentes refinados e três placas novas, atlas e prefabs.
- Networking/Editor/CityLifeInstaller.cs: importa revisão ativa e compõe o chão visual.
- Networking/Tests/CityLifeTests.cs: atlas/materiais e normais das vias, além das verificações anteriores.
- Data/World/Town/Surroundings/CityMeadow.mat, CityPaths.mat e CityPaths.asset.
- Scenes/RefugioUltimoSino.unity; navegação recalculada após integrar como nas revisões anteriores.
- Networking/Editor/CampaignInstaller.cs e ProjectSettings: versão 0.30.0; código Android futuro 53.

## Testar
Entrar na cidade na build Windows atual. Conferir superfície das barracas e dos arcos, símbolos das três placas, distinção entre praça/caminhos/jardins e legibilidade pelo zoom. Percorrer saída, fonte e pátio de expedições para conferir comportamento preservado. A aparência ainda é uma proposta estilizada; não corresponde ao acabamento final da referência aprovada.

## Limitações
Não há novo combate, missão, evento, comércio, NPC final ou modelo de inimigo. Os sinais são decorativos; serviços reais anteriores permanecem como antes. Nenhum APK ou teste Android/online nesta etapa. Fonte base e revisão ativa coexistem de propósito para recuperação, com seus papéis indicados no índice; não são executáveis adicionais. Validação, build e capturas em Docs/Validation/0.30.0.

Validação: 36/36 FBX reimportados, 24/24 testes Unity passaram, build Windows errors=0. Conferência visual em execução visível 1280x800; áudio desativado apenas nessa sessão QA. Capturas preservadas. Persistem transições de textura no encontro entre pavimentos, acabamento dos modelos e mistura com assets antigos; não representam bloqueio de navegação nem estilo final aprovado.
