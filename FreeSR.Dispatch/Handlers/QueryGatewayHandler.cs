namespace FreeSR.Dispatch.Handlers
{
    using Ceen;
    using FreeSR.Dispatch.Util;
    using FreeSR.Proto;
    using System.Threading.Tasks;

    internal class QueryGatewayHandler : IHttpModule
    {
        public async Task<bool> HandleAsync(IHttpContext context)
        {
            context.Response.StatusCode = HttpStatusCode.OK;
            context.Response.ContentType = "text/plain";
            await context.Response.WriteAllAsync(Convert.ToBase64String(ProtobufUtil.Serialize(new Gateserver
            {
                Retcode = 0,
                Msg = "OK",
                Ip = "127.0.0.1",
                //RegionName = "FreeSR",
                Port = 23301,
                Pdpbjhfgnjk = true,
                Bipcmeeljhj = true,
                Hecpclndaac = true,
                Nlfkefmfige = true,
                Oigmgpfnloj = true,
                Pnnionnkbnn = true,
                UseTcp = true,
                //MdkResVersion = "5335706",
                AssetBundleUrl = "https://autopatchos.starrails.com/asb/BetaLive/output_7037158_b67f5a6a68fb",
                ExResourceUrl = "https://autopatchos.starrails.com/design_data/BetaLive/output_7033392_aaca9c1b456b",
                IfixVersion = "0",
                LuaUrl = "https://autopatchos.starrails.com/lua/BetaLive/output_7050564_f05a0f949b10",
                LuaVersion = "7050564"
            })));

            return true;
        }
    }
}
