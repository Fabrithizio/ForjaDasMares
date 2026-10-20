namespace ForjaDasMares
{
    public static class CombatPerkDescriptions
    {
        public static string Perk(WeaponPerk value)=>value==WeaponPerk.Bleed?"Sangramento por 2,5s. Reaplicar renova, não acumula cargas.":value==WeaponPerk.Scorch?"Queima por 2,5s. Dano periódico limitado; reaplicar renova.":value==WeaponPerk.Fracture?"Fratura: reduz a defesa atual do inimigo em 25% por 3s e interrompe brevemente.":value==WeaponPerk.Pull?"Puxa inimigos próximos. Máximo uma aplicação por alvo a cada 0,8s.":"Sem efeito especial.";
        public static string Synergy(BuildSynergy value)=>((value&BuildSynergy.ScorchedEdge)!=0?"Brasa viva: +20% contra alvos queimando; uma morte por golpe espalha queimadura em 2,5m. ":"")+((value&BuildSynergy.ControlHunter)!=0?"Caçador: +15% contra alvos lentos, enraizados ou atordoados. ":"")+((value&BuildSynergy.KindredGuard)!=0?"Vínculo: ao abater, cura aliados próximos em 4; máximo 4 ativações por expedição, intervalo de 3s.":"");
        public static string Boon(int index)=>index==0?"PODER · +12% de dano nesta expedição":index==1?"RITMO · +10% de cadência nesta expedição":"ABRIGO · 6% de proteção nesta expedição";
    }
}
