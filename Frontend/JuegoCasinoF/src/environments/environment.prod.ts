// Build de producción (APK de Android): la API se expone con un Dev Tunnel persistente de
// Visual Studio, así el celular llega a la laptop desde cualquier red.
// Para que la app funcione, la API debe estar corriendo con el túnel activo.
export const environment = {
  production: true,
  apiUrl: 'https://zqs00g5p-7187.use2.devtunnels.ms/api',
  hubBlackjackUrl: 'https://zqs00g5p-7187.use2.devtunnels.ms/hubs/blackjack'
};
