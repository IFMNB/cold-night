## Объект, выполняющий специальные сценарные скрипты
class_name Scene
extends Node

## Обычно сцена удаляется после выполнения вместе со своими скриптами, но можно переопределить это поведение если нужно
@export var Once: bool = true
## Ссылка на сценарий, который нужно выполнить
@export var Execute: Array[ScenarioScript]
## Вызов сценариев обычно заставляет их выполняться асинхронно друг другу, этот параметр заставляет дождаться конца вызванной coroutine перед следующей
@export var Async: bool = false

func DoScenarios ():
	for i in Execute:
		var e: ScenarioScript
		
		if (i.Recreate):	
			e = i.duplicate(i.DuplicateSubresources)
		else:
			e = i
		
		if (Async):
			await e.main(self)
		else:
			e.main(self)
	
	pass

func _ready ():
	DoScenarios()

	if self.Once:
		self.free()
		
	pass;

func _process(delta: float) -> void:
	DoScenarios()
	
	pass
