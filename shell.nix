let
  nixpkgs = import <nixpkgs> {};
  inherit (nixpkgs) pkgs stdenv;
in
stdenv.mkDerivation {
  name = "FlagsDemo";

  buildInputs = [
    pkgs.dotnet-sdk_8
    pkgs.dotnet-aspnetcore_8
  ];
}
