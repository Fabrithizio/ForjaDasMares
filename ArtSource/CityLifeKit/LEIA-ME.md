# Kit Cidade Viva — modelos originais

06/11/2026. Blender 4.5.14 LTS. Fontes editáveis em Cidade_viva_editavel.blend; gerador em Ferramentas/gerar_cidade_viva.py. A biblioteca geométrica do SettlementKit é reutilizada sem executar os trabalhos antigos nem sobrescrever seus modelos.

15 modelos de conteúdo, cada um com dois LODs: portão aberto da cidade, folha de porta separada com pivô na dobradiça, fachada de estação invadida por raízes, protetor da praça, pavilhão de eventos, quadro de avisos, bandeirolas, raízes envolvendo máquina, arco de jardim e seis barracas (frutas, padeiro, ervas, ferramentas, tecidos e relíquias).

São 30 FBX, não 30 assets distintos. As barracas têm silhueta construtiva comum e cores/mercadorias diferentes; não são seis sistemas de comércio. O acabamento usa geometria de baixa contagem e paleta compartilhada, não texturas pintadas finais. Cada mesh tem um material/submesh e UV de paleta; não é unwrap para pintura PBR detalhada.

Previews contém renderizações reais de cada modelo e PRANCHA.png. Documentacao/CATALOGO.csv tem dimensões em metros e triângulos; validacao_fbx.json registra reimportação dos 30 FBX. Não gerar UV2 ou texturas enormes automaticamente sem necessidade de iluminação baked.

Reprodução: executar o Blender em modo --background --factory-startup --disable-autoexec --threads 2 --python ArtSource/CityLifeKit/Ferramentas/gerar_cidade_viva.py a partir da raiz do projeto; depois validar com Ferramentas/validar_kit.py. O gerador substitui somente este kit; salvar cópia antes de alterações manuais. A execução é independente da sessão Blender aberta do usuário.

Na Unity: Assets/ForjaDasMares/Art/World/CityLifeKit. Prefabs com LODGroup, material URP compartilhado e colisores simples específicos; arcos mantêm vãos abertos. A folha de portão está preparada como peça separada, mas não tem animação nem lógica de abertura. Não importar o .blend no Assets: isso evitará dependência do Blender para abrir/compilar Unity.

Não inclui inimigos novos, NPCs finais, rig, animações, comércio, evento ou missão novos. A fachada temática não implementa o desafio, seus inimigos ou recompensas. A lógica antiga de expedições continua conectada ao acesso existente.
