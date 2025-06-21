function obtenerRecomendacion(peliculaId, titulo, generos) {
    fetch('/recomendacion/recomendar', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ peliculaId: peliculaId, titulo: titulo, generos: generos }) // ajustá el usuarioId
    })
        .then(response => response.json())
        .then(data => {
            mostrarPopup(data.mensaje, data.score, data.titulo, data.scoreFinal);
        })
        .catch(error => {
            console.error('Error:', error);
        });
}

function mostrarPopup(mensaje, score, titulo, scoreTotal) {
    const contenedor = document.getElementById("popup-recomendacion");
    contenedor.innerHTML = `
               <div class="popup" style="position: fixed; top: 20px; left: 50%; transform: translateX(-50%);
                 background-color: #fefefe; border: 2px solid #4a90e2; padding: 20px; border-radius: 10px;
                 box-shadow: 0 4px 10px rgba(0,0,0,0.2); z-index: 999; min-width: 300px; max-width: 90%;">

                <div style="display: flex; justify-content: space-between; align-items: center;">                    
                    <button onclick="cerrarPopup()" style="background: none;position:fixed; right:15px; border: none; font-size: 1.2rem; cursor: pointer;">❌</button>
                </div>

                <hr />

                <p><strong>🎬 Título:</strong> ${titulo}</p>
                <p><strong>💬 Recomendación:</strong> ${mensaje}</p>
                <p><strong>⭐ Score estimado:</strong> ${score.toFixed(2)}</p>
                <p><strong>⭐ Score usuario:</strong> ${scoreTotal.toFixed(2)}</p>
            </div>
        `;
    contenedor.style.display = "block";
}

function cerrarPopup() {
    document.getElementById("popup-recomendacion").style.display = "none";
}