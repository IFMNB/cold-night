## Специальный GodotScript объект, который будет выполнен как независимый чанк кода.
## Выполняется в среде Scene
class_name ScenarioScript
extends Resource

## После выполнения этот скрипт, обычно, требуется пересоздавать. Если нужно сохранять какое-либо состояние между кадрами, то отключите это свойство
@export var Recreate: bool = true
## Некоторые ресурсы могут оставаться после выполнения, следует ли учитывать их?
@export var DuplicateSubresources: bool = true

func main (scene: Scene):
	pass
