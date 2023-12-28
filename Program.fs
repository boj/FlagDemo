open System
open System.Collections.Generic
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

open Giraffe
open Unleash
open Unleash.ClientFactory

let settings = UnleashSettings(
            AppName = "FlagDemo",
            UnleashApi = Uri("http://localhost:4242/api/"),
            CustomHttpHeaders = Dictionary(
                dict [(
                    "Authorization",
                    "default:development.unleash-insecure-api-token"
                )]
            ),
            FetchTogglesInterval = TimeSpan.FromSeconds(5)
        )
let unleashFactory = new UnleashClientFactory()
let unleash : IUnleash = unleashFactory.CreateClientAsync(settings, synchronousInitialization = true)
                             |> Async.AwaitTask
                             |> Async.RunSynchronously

let flagA : HttpHandler =
    fun _ ctx ->
        let u = ctx.GetService<IUnleash>()
        let v = if u.IsEnabled "feature_flag_a" then
                     """
                       <label>Flag A:</label>
                       <div id="flag-a" class="rpgui-icon sword"></div>
                     """
                 else
                     ""
        ctx.WriteStringAsync(v)

let flagB : HttpHandler =
    fun _ ctx ->
        let u = ctx.GetService<IUnleash>()
        let v = if u.IsEnabled "feature_flag_b" then
                    """
                      <label>Flag B:</label>
                      <div id="flag-b" class="rpgui-icon shield"></div>
                    """
                else
                    ""
        ctx.WriteStringAsync(v)

let webApp =
    choose [
        route "/" >=> htmlFile "wwwroot/index.html"
        route "/flags/a" >=> flagA
        route "/flags/b" >=> flagB 
    ]

let configureApp (app : IApplicationBuilder) =
    app.UseStaticFiles()
       .UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore
    services.AddSingleton<IUnleash>(unleash) |> ignore
    
[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    unleash.Dispose()
    0
