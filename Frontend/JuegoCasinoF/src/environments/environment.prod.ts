// Build de producción (APK de Android): la API se expone con un Dev Tunnel persistente de
// Visual Studio, así el celular llega a la laptop desde cualquier red.
// Para que la app funcione, la API debe estar corriendo con el túnel activo.
export const environment = {
  production: true,
  apiUrl: 'https://bzlrtsd5-7187.use.devtunnels.ms/api',
  hubBlackjackUrl: 'https://bzlrtsd5-7187.use.devtunnels.ms/hubs/blackjack'
};
