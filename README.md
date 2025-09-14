# Sistema de Comunicación Multicast en C#

## Descripción

Este proyecto implementa un sistema de comunicación **multicast** entre nodos de un grupo, coordinado por un **Coordinador** y con posibilidad de enviar mensajes desde un **Nodo Externo**.  

El sistema cuenta con tres componentes principales:

1. **Coordinador**:  
   - Recibe mensajes de los clientes externos.  
   - Reenvía los mensajes a todos los nodos del grupo.  
   - Espera que cada nodo confirme la recepción con un **ACK**.  
   - Responde al Nodo Externo indicando si todos los nodos recibieron el mensaje.

2. **NodoGrupo**:  
   - Se conecta al Coordinador.  
   - Solo recibe mensajes del Coordinador.  
   - Devuelve un ACK para confirmar recepción.

3. **NodoExterno**:  
   - Se conecta al Coordinador.  
   - Permite enviar mensajes al grupo.  
   - Recibe confirmación del Coordinador (ACK o ERROR).

---

## Diagrama de relaciones

<img width="693" height="441" alt="image" src="https://github.com/user-attachments/assets/b6dcc9bb-49c2-4dec-b4f0-12df7a6192c9" />

---

## Cómo ejecutar

### Pasos

**1. Ir hasta la solución (.sln)**

**2. Abrir el **Coordinador** y ejecutarlo primero:**
   ```bash
   cd Coordinador/bin/Debug/net8.0
   dotnet Coordinador.dll
   ```
  Esto levantará dos listeners:
  * Puerto 5000 → para los NodosGrupo
  * Puerto 6000 → para los NodosExternos

**3. En otra consola, abrir la cantidad de nodos del grupo deseados y ejecutarlos (se ha de abrir una consola para cada uno):**
```bash
   cd NodoGrupo/bin/Debug/net8.0
   dotnet NodoGrupo.dll
   ```
  Cada Nodo se conectará al Coordinador en el puerto 5000 y esperará mensajes.

**4. Abrir el nodo externo:**
```bash
   cd NodoExterno/bin/Debug/net8.0
   dotnet NodoExterno.dll
   ```
  En este último se podrá escribir mensajes en la consola, los cuales se enviarán al Coordinador que lo repartirá a todos los nodos.
  El nodo externo recibirá un ACK si todos los nodos confirmaron, o un mensaje de ERROR indicando fallas.
  
  Para desconectar el nodo externo es suficiente con escribir CLOSE

> Nota: los nodos del grupo siguen escuchando hasta que el coordinador se cierre o se interrumpa manualmente.
