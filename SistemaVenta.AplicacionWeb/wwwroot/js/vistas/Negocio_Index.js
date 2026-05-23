$(document).ready(function () {

    // Cargar catálogo de Régimen Fiscal
    fetch("/Negocio/ListaRegimenFiscal")
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboRegimenFiscal").append(
                        $("<option>").val(item.idRegimenFiscal).text(`${item.cRegimenFiscal} - ${item.descripcion}`)
                    );
                });
            }
        });

    // Cargar datos del negocio
    $(".card-body").LoadingOverlay("show");

    fetch("/Negocio/Obtener")
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                const d = responseJson.objeto;

                $("#txtRfc").val(d.rfc);
                $("#txtRazonSocial").val(d.nombre);
                $("#txtCorreo").val(d.correo);
                $("#txtDireccion").val(d.direccion);
                $("#txTelefono").val(d.telefono);
                $("#txtCodigoPostal").val(d.codigopostal);
                $("#txtSimboloMoneda").val(d.simboloMoneda);
                $("#imgLogo").attr("src", d.urlLogo);

                if (d.idRegimenFiscal) {
                    $("#cboRegimenFiscal").val(d.idRegimenFiscal);
                }
            } else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        });
});

$("#btnGuardarCambios").click(function () {

    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    if (inputs_sin_valor.length > 0) {
        toastr.warning("", `Debe completar el campo: "${inputs_sin_valor[0].name}"`);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return;
    }

    const modelo = {
        rfc: $("#txtRfc").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        codigopostal: $("#txtCodigoPostal").val(),
        simboloMoneda: $("#txtSimboloMoneda").val(),
        idRegimenFiscal: parseInt($("#cboRegimenFiscal").val()) || null,
        porcentajeImpuesto: "0"
    };

    const inputLogo = document.getElementById("txtLogo");
    const formData = new FormData();
    formData.append("logo", inputLogo.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    $(".card-body").LoadingOverlay("show");

    fetch("/Negocio/GuardarCambios", {
        method: "POST",
        body: formData
    })
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                swal("¡Listo!", "Los cambios del negocio fueron guardados.", "success");
                $("#imgLogo").attr("src", responseJson.objeto.urlLogo);
            } else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        });
});
