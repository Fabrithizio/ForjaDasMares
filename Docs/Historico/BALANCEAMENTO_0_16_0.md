# Critérios de balanceamento e legibilidade — 0.16.0

Revisão inicial dos dados reais; não representa balanceamento final nem uma vitória humana medida.

- Inicial do Arcanista: 72 de vida, arma inicial com 9 de dano, um alvo e nenhuma explosão. Testes Prisma verificam que cajado, grimório e foco mágico mantêm padrões/controles diferentes e restritos à classe.
- Vida adicional de equipamento: soma limitada a 160; dano adicional a 50%; chance crítica a 30%; velocidade de ataque adicional a 45%; recarga adicional a 35%; elemental a 25%; regeneração a 2 vida/s. Defesa segue limite do catálogo. O combate limita a cadência multiplicativa a 2,5x. Não aumentar esses limites sem medir efeitos em conjunto.
- Exemplo observado no perfil de QA de nível9: Corta-Maré rara levou dano14→26, vida165→175 e cadência1,78→2,58 golpes/s; a retirada restaurou os números e o espaço na mochila. Essa comparação não significa que a espada é universalmente superior a outras armas.
- Encontro inicial Wilderness/Horde_0_0 tem duas ondas de 12 inimigos, reforço quando restam até2 e limites configurados de20 no móvel/24 no PC. O número total de uma sequência é diferente da simultaneidade.
- Nesta atualização, o ajuste de justiça é temporal/espacial: aviso de entrada mínimo0,6s e afastamento de1,8m de jogadores. Não houve aumento indiscriminado de vida/dano.

## Sessões necessárias antes de alterar atributos novamente
1. Partida nova, sem itens herdados: tempo para primeiro inimigo, primeiro drop e primeiro equipamento útil.
2. Cada classe/arma: dano por janela de ataque, número de esquivas necessárias, tempo de exposição e valor dos efeitos de controle.
3. Solo e dupla: tempo por encontro, mortes, cura disponível, ouro/XP/raridades e espaços consumidos. Repetir com a mesma seed e níveis comparáveis.
4. Chefes: cada ataque deve ser reconhecido e evitável; verificar recuperação/vulnerabilidade sem depender de dano excessivo.
5. Build especializada deve ter trocas compreensíveis. Evitar classificar toda perda de cadência ou aumento de alcance como irrelevante.
6. Dificuldade deve crescer pela composição e comportamento; só ajustar atributos quando houver evidência de um problema, registrando antes/depois.

A validação automatizada de dano/controle/limites ajuda a evitar regressões, mas não substitui teste humano de diversão ou dificuldade.
