
import sys
from os import path

current_dir = path.dirname(path.abspath(__file__))
node_dir = current_dir
while path.split(current_dir)[-1] != r'Heron':
    current_dir = path.dirname(current_dir)
sys.path.insert(0, path.dirname(current_dir))

from Heron.communication.socket_for_serialization import Socket
from Heron import general_utils as gu, constants as ct
import commands_to_unity as cu

screen_res = []
observation_type: str
initialised = False


def get_parameters(_worker_object):
    global screen_res
    global observation_type

    try:
        parameters = _worker_object.parameters
        screen_res_x = parameters[0]
        screen_res_y = parameters[1]
        observation_type = parameters[2]
    except:
        return False

    screen_res.append(screen_res_x)
    screen_res.append(screen_res_y)

    return True


def initialise(_worker_object):
    global initialised

    if not get_parameters(_worker_object):
        return False

    if not cu.connect_sockets():
        return False

    if not cu.start_unity_exe(node_dir):
        return False

    if not cu.first_communication_with_unity(screen_res):
        return False

    initialised = True
    return True


def work_function(data, parameters, savenodestate_update_substate_df):


    topic = data[0]

    message = data[1:]  # data[0] is the topic
    message = Socket.reconstruct_array_from_bytes_message(message)

    #savenodestate_update_substate_df(image__shape=message.shape)


    result = [message]

    return result


# The on_end_of_life function must exist even if it is just a pass
def on_end_of_life():
    if initialised:
        cu.kill_unity()


if __name__ == "__main__":
    worker_object = gu.start_the_transform_worker_process(work_function=work_function,
                                                          end_of_life_function=on_end_of_life,
                                                          initialisation_function=initialise)
    worker_object.start_ioloop()
